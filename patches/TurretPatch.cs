using UnityEngine;
using HarmonyLib;
using Unity.Netcode;
using GameNetcodeStuff;

namespace AndysModsPlugin.patches
{
    //[HarmonyPatch(typeof(EnemyAI))]
    internal static class EnemyPatch
    {
        //[HarmonyPatch("Start")]
        //[HarmonyPrefix]
        internal static void Start(EnemyAI __instance)
        {
            AndysModsPlugin.Log.LogInfo("Turrets Are No Joke: Registered playerControllerB to enable turret targeting.");
            __instance.gameObject.AddComponent<PlayerControllerB>();
        }
    }

    //[HarmonyPatch(typeof(Turret))]
    internal static class TurretPatch
    {
        //[HarmonyPatch("Update")]
        //[HarmonyPostfix]
        internal static void Update(Turret __instance)
        {
            AndysModsPlugin.Log.LogInfo($"Turrets Are No Joke: Called update for turret instance. Current target is {__instance.targetPlayerWithRotation?.name}");
            EnemyAI ai;
            if (__instance.targetPlayerWithRotation != null && __instance.gameObject.TryGetComponent(out ai))
            {
                IHittable hittable;
                ai.TryGetComponent(out hittable);
                RaycastHit hit = (RaycastHit)Traverse.Create(__instance).Field("hit").GetValue();
                float distanceToTarget = Vector3.Distance(__instance.centerPoint.position, hit.point);
                int damageDealt = distanceToTarget < 5f ? 5 : 3;
                hittable?.Hit(damageDealt, __instance.centerPoint.position, null, playHitSFX: true);
            }
            //__instance.gameObject.AddComponent<FriendlyFireTurretBehaviour>().Initialize(__instance);
        }

        //[HarmonyPatch("Start")]
        //[HarmonyPostfix]
        internal static void Start()
        {
            AndysModsPlugin.Log.LogInfo("Turrets Are No Joke: Added turret to the map.");
        }
    }

    internal class FriendlyFireTurretBehaviour : NetworkBehaviour
    {
        private Turret turret;
        private EnemyAI targetEnemyAi;
        private IHittable hittable;
        private Ray shootRay;
        private RaycastHit hit;
        private bool hasLineOfSight;
        private float lostLOSTimer;
        private float turretInterval;
        //private Traverse turretModeLastFrame;
        //private Traverse rotatingClockwise;
        //private Traverse rotatingSmoothly;
        //private Traverse fadeBulletAudioCoroutine;
        //private Traverse switchRotationTimer;
        //private Traverse rotatingRight;
        public void Initialize(Turret turret)
        {
            this.turret = turret;
            AndysModsPlugin.Log.LogInfo("Turrets Are No Joke: Added GameComponent to turret.");
            Traverse turretTraverse = Traverse.Create(turret);
            //turretModeLastFrame = turretTraverse.Field("turretModeLastFrame");
            //rotatingClockwise = turretTraverse.Field("rotatingClockwise");
            //rotatingSmoothly = turretTraverse.Field("rotatingSmoothly");
            //fadeBulletAudioCoroutine = turretTraverse.Field("fadeBulletAudioCoroutine");
            //switchRotationTimer = turretTraverse.Field("switchRotationTimer");
            //rotatingRight = turretTraverse.Field("rotatingRight");
        }

        public EnemyAI CheckForEnemiesInLineOfSight(float radius = 2f)
        {
            Vector3 forward = turret.aimPoint.forward;
            forward = Quaternion.Euler(0f, (int)(0f - turret.rotationRange) / radius, 0f) * forward;
            float num = turret.rotationRange / radius * 2f;
            for (int i = 0; i <= 6; i++)
            {
                shootRay = new Ray(turret.centerPoint.position, forward);
                if (Physics.Raycast(shootRay, out hit, 30f, 524288, QueryTriggerInteraction.Collide))
                {
                    EnemyAICollisionDetect collidedEnemy = hit.transform.GetComponent<EnemyAICollisionDetect>();
                    if (collidedEnemy == null) { continue; }
                    IHittable component;
                    if (collidedEnemy.transform.TryGetComponent(out component))
                    {
                        EnemyAI enemy = collidedEnemy.mainScript;
                        hittable = component;
                        if (enemy != null && enemy.enemyType.canDie && !enemy.isEnemyDead)
                        {
                            return enemy;
                        }

                        continue;
                    }
                }
                forward = Quaternion.Euler(0f, num / 6f, 0f) * forward;
            }

            return null;
        }
        private void SetTargetToEnemyBody()
        {
            turret.targetTransform = targetEnemyAi?.transform.parent.transform;
        }

        private void TurnTowardsTargetIfHasLOS()
        {
            bool flag = true;
            if (targetEnemyAi == null || targetEnemyAi.isEnemyDead || Vector3.Angle(turret.targetTransform.position - turret.centerPoint.position, turret.forwardFacingPos.forward) > turret.rotationRange)
            {
                flag = false;
            }

            if (Physics.Linecast(turret.aimPoint.position, turret.targetTransform.position, StartOfRound.Instance.collidersAndRoomMask, QueryTriggerInteraction.Ignore))
            {
                flag = false;
            }

            if (flag)
            {
                hasLineOfSight = true;
                lostLOSTimer = 0f;
                turret.tempTransform.position = turret.targetTransform.position;
                turret.tempTransform.position -= Vector3.up * 0.15f;
                turret.turnTowardsObjectCompass.LookAt(turret.tempTransform);
                return;
            }

            if (hasLineOfSight)
            {
                hasLineOfSight = false;
                lostLOSTimer = 0f;
            }

            if (!IsServer)
            {
                return;
            }

            lostLOSTimer += Time.deltaTime;
            if (lostLOSTimer >= 1f)
            {
                lostLOSTimer = 0f;
                EnemyAI enemy = CheckForEnemiesInLineOfSight();
                if (enemy != null)
                {
                    targetEnemyAi = enemy;
                    SwitchTargetedEnemyClientRpc(enemy);
                }
                else
                {
                    targetEnemyAi = null;
                    RemoveTargetedEnemyClientRpc();
                }
            }
        }
        public void Update()
        {
            if (turret.targetPlayerWithRotation != null)
            {
                return;
            }

            if (!turret.turretActive)
            {
                turret.turretMode = TurretMode.Detection;
                targetEnemyAi = null;
                return;
            }

            if (targetEnemyAi != null)
            {
                if (targetEnemyAi.isEnemyDead || !hasLineOfSight)
                {
                    targetEnemyAi = null;
                    RemoveTargetedEnemyClientRpc();
                    turret.turretMode = TurretMode.Detection;
                }
                else
                {
                    turret.turretMode = TurretMode.Charging;
                    SetTargetToEnemyBody();
                    TurnTowardsTargetIfHasLOS();
                }
            }

            switch (turret.turretMode)
            {
                case TurretMode.Detection:
                    if (!turret.IsServer)
                    {
                        break;
                    }
                    if (turretInterval >= 0.25f)
                    {
                        turretInterval = 0f;
                        EnemyAI enemy = CheckForEnemiesInLineOfSight(2f);
                        if (enemy != null)
                        {
                            targetEnemyAi = enemy;
                            turret.turretMode = TurretMode.Charging;
                            SwitchTargetedEnemyClientRpc(enemy, setModeToCharging: true);
                        }
                    }
                    else
                    {
                        turretInterval += Time.deltaTime;
                    }
                    break;
                case TurretMode.Charging:
                    if (!turret.IsServer)
                    {
                        break;
                    }
                    if (turretInterval >= 1.0f)
                    {
                        turretInterval = 0f;
                        AndysModsPlugin.Log.LogInfo("Turrets Are No Joke: Charging timer is up, setting to firing mode");
                        if (!hasLineOfSight)
                        {
                            targetEnemyAi = null;
                            RemoveTargetedEnemyClientRpc();
                            turret.turretMode = TurretMode.Detection;
                        }
                        else
                        {
                            turret.turretMode = TurretMode.Firing;
                            turret.SetToModeClientRpc(2);
                        }
                    }
                    else
                    {
                        turretInterval += Time.deltaTime;
                    }
                    break;
                case TurretMode.Firing:
                    if (turretInterval >= 0.21f)
                    {
                        turretInterval = 0f;
                        EnemyAI newEnemyTarget = CheckForEnemiesInLineOfSight(3f);
                        if (newEnemyTarget != null)
                        {
                            AndysModsPlugin.Log.LogInfo($"Turrets Are No Joke: Re-targeting to new enemy {newEnemyTarget.name}.");
                            turret.targetPlayerWithRotation = null;
                            targetEnemyAi = newEnemyTarget;
                            SetTargetToEnemyBody();
                            TurnTowardsTargetIfHasLOS();
                        }
                        float distanceToTarget = Vector3.Distance(turret.centerPoint.position, hit.point);
                        int damageDealt = distanceToTarget < 5f ? 5 : 3;
                        hittable?.Hit(damageDealt, turret.centerPoint.position, null, playHitSFX: true);
                    }
                    else
                    {
                        turretInterval += Time.deltaTime;
                    }
                    break;
            }

        }

        [ClientRpc]
        public void SwitchTargetedEnemyClientRpc(EnemyAI ai, bool setModeToCharging = false)
        {
            NetworkManager networkManager = NetworkManager;
            if ((object)networkManager == null || !networkManager.IsListening)
            {
                return;
            }

            if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
            {
                ClientRpcParams clientRpcParams = default;
                FastBufferWriter bufferWriter = __beginSendClientRpc(866050294u, clientRpcParams, RpcDelivery.Reliable);
                BytePacker.WriteValuePacked(bufferWriter, ai);
                bufferWriter.WriteValueSafe(in setModeToCharging, default);
                __endSendClientRpc(ref bufferWriter, 866050294u, clientRpcParams, RpcDelivery.Reliable);
            }

            if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost) && !IsServer)
            {
                targetEnemyAi = ai;
                if (setModeToCharging)
                {
                    turret.turretMode = TurretMode.Charging;
                }
            }
        }
        [ClientRpc]
        public void RemoveTargetedEnemyClientRpc()
        {
            NetworkManager networkManager = NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
                {
                    ClientRpcParams clientRpcParams = default;
                    FastBufferWriter bufferWriter = __beginSendClientRpc(2800017671u, clientRpcParams, RpcDelivery.Reliable);
                    __endSendClientRpc(ref bufferWriter, 2800017671u, clientRpcParams, RpcDelivery.Reliable);
                }

                if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost))
                {
                    targetEnemyAi = null;
                }
            }
        }
    }
}