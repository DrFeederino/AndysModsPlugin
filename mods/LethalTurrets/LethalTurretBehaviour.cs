using GameNetcodeStuff;
using HarmonyLib;
using System.Collections;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AndysModsPlugin.mods.LethalTurrets
{
    internal class LethalTurretBehaviour : NetworkBehaviour
    {
        private EnemyAI targetEnemy;
        private bool targetingDeadEnemy;
        private bool wasTargetingEnemyLastFrame;
        private Turret turret;
        private Traverse fieldTraverser;
        private RaycastHit hit;
        private static int TurretLayerMask = 11012424;

        public void HookupTurret(Turret turret)
        {
            this.turret = turret;
            fieldTraverser = Traverse.Create(turret);
            turret.enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            AndysModsPlugin.Log.LogInfo("Lethal Turrets: spawned network object for turret!");
            base.OnNetworkSpawn();
        }

        private void SwitchTurretMode(int mode)
        {
            turret.turretMode = (TurretMode)mode;
        }

        private void SetTargetToPlayerBody()
        {
            MethodInfo dynMethod = turret.GetType().GetMethod("SetTargetToPlayerBody", BindingFlags.NonPublic | BindingFlags.Instance);
            dynMethod.Invoke(turret, []);
        }

        private void TurnTowardsTargetIfHasLOS()
        {
            MethodInfo dynMethod = turret.GetType().GetMethod("TurnTowardsTargetIfHasLOS", BindingFlags.NonPublic | BindingFlags.Instance);
            dynMethod.Invoke(turret, []);
        }

        private IEnumerator FadeBulletAudio()
        {
            float initialVolume = turret.bulletCollisionAudio.volume;
            for (int i = 0; i <= 30; i++)
            {
                yield return new WaitForSeconds(0.012f);
                turret.bulletCollisionAudio.volume = Mathf.Lerp(initialVolume, 0f, i / 30f);
            }

            turret.bulletCollisionAudio.Stop();
        }

        public EnemyAI CheckForEnemyInSight(float radius = 2f)
        {
            Vector3 forward = turret.aimPoint.forward;
            forward = Quaternion.Euler(0f, (0f - turret.rotationRange) / radius, 0f) * forward;
            float num = turret.rotationRange / radius * 2f;
            for (int i = 0; i <= 6; i++)
            {
                fieldTraverser.Field<Ray>("shootRay").Value = new Ray(turret.centerPoint.position, forward);
                if (Physics.Raycast(fieldTraverser.Field<Ray>("shootRay").Value, out hit, 30f, TurretLayerMask, QueryTriggerInteraction.Collide))
                {
                    if (!hit.transform.TryGetComponent(out EnemyAICollisionDetect enemyAICollision))
                    {
                        return null;
                    }

                    EnemyAI component = enemyAICollision.mainScript;
                    if (component != null && !component.isEnemyDead && component.enemyType.canDie)
                    {
                        return component;
                    }

                    continue;
                }

                forward = Quaternion.Euler(0f, num / 6f, 0f) * forward;
            }

            return null;
        }

        private void SetTargetToEnemy()
        {
            if (targetEnemy.isEnemyDead)
            {
                if (!targetingDeadEnemy)
                {
                    targetingDeadEnemy = true;
                }

            }
            else
            {
                targetingDeadEnemy = false;
                turret.targetTransform = targetEnemy.gameObject.transform;
            }
        }

        private void TurnTowardsTargetEnemyIfHasLos()
        {
            bool flag = true;
            if (targetingDeadEnemy || Vector3.Angle(turret.targetTransform.position - turret.centerPoint.position, turret.forwardFacingPos.forward) > turret.rotationRange)
            {
                flag = false;
            }

            if (Physics.Linecast(turret.aimPoint.position, turret.targetTransform.position, StartOfRound.Instance.collidersAndRoomMask, QueryTriggerInteraction.Ignore))
            {
                flag = false;
            }

            if (flag)
            {
                fieldTraverser.Field<bool>("hasLineOfSight").Value = true;
                fieldTraverser.Field<float>("lostLOSTimer").Value = 0f;

                turret.tempTransform.position = turret.targetTransform.position;
                turret.tempTransform.position += Vector3.up * 0.6f;
                turret.turnTowardsObjectCompass.LookAt(turret.tempTransform);
                return;
            }

            if (fieldTraverser.Field<bool>("hasLineOfSight").Value)
            {
                fieldTraverser.Field<bool>("hasLineOfSight").Value = false;
                fieldTraverser.Field<float>("lostLOSTimer").Value = 0f;
            }

            if (!IsServer)
            {
                return;
            }

            fieldTraverser.Field<float>("lostLOSTimer").Value += Time.deltaTime;
            if (fieldTraverser.Field<float>("lostLOSTimer").Value >= 2f)
            {
                fieldTraverser.Field<float>("lostLOSTimer").Value = 0f;
                EnemyAI enemy = CheckForEnemyInSight();
                turret.targetPlayerWithRotation = null;
                if (enemy != null)
                {
                    targetEnemy = enemy;
                    SwitchTargetedEnemyClientRpc(enemy.NetworkObjectId);
                }
                else
                {
                    targetEnemy = null;
                    RemoveTargetedEnemyClientRpc();
                }
            }
        }
        private void Update()
        {
            if (turret == null)
            {
                AndysModsPlugin.Log.LogInfo("Lethal turrets: tried to call on NULL turret.");
                if (NetworkObject.IsSpawned)
                {
                    AndysModsPlugin.Log.LogInfo("Lethal turrets: despawned network object for client.");
                    NetworkObject.Despawn(true);
                }
                gameObject.SetActive(false);
                Destroy(this);
                return;
            }
            if (!turret.turretActive)
            {
                fieldTraverser.Field<bool>("wasTargetingPlayerLastFrame").Value = false;
                wasTargetingEnemyLastFrame = false;
                turret.turretMode = TurretMode.Detection;
                turret.targetPlayerWithRotation = null;
                targetEnemy = null;
                return;
            }

            if (targetEnemy != null)
            {
                if (!wasTargetingEnemyLastFrame)
                {
                    wasTargetingEnemyLastFrame = true;
                    if (turret.turretMode == TurretMode.Detection)
                    {
                        turret.turretMode = TurretMode.Charging;
                    }
                }

                SetTargetToEnemy();
                TurnTowardsTargetEnemyIfHasLos();
            }
            else if (turret.targetPlayerWithRotation != null)
            {
                if (!fieldTraverser.Field<bool>("wasTargetingPlayerLastFrame").Value)
                {
                    fieldTraverser.Field<bool>("wasTargetingPlayerLastFrame").Value = true;
                    if (turret.turretMode == TurretMode.Detection)
                    {
                        turret.turretMode = TurretMode.Charging;
                    }
                }

                SetTargetToPlayerBody();
                TurnTowardsTargetIfHasLOS();
            }

            else if (fieldTraverser.Field<bool>("wasTargetingPlayerLastFrame").Value)
            {
                fieldTraverser.Field<bool>("wasTargetingPlayerLastFrame").Value = false;
                turret.turretMode = TurretMode.Detection;
            }
            else if (wasTargetingEnemyLastFrame)
            {
                wasTargetingEnemyLastFrame = false;
                turret.turretMode = TurretMode.Detection;
            }

            switch (turret.turretMode)
            {
                case TurretMode.Detection:
                    if (fieldTraverser.Field<TurretMode>("turretModeLastFrame").Value != 0)
                    {
                        fieldTraverser.Field<TurretMode>("turretModeLastFrame").Value = TurretMode.Detection;
                        fieldTraverser.Field<bool>("rotatingClockwise").Value = false;
                        turret.mainAudio.Stop();
                        turret.farAudio.Stop();
                        turret.berserkAudio.Stop();
                        if (fieldTraverser.Field<Coroutine>("fadeBulletAudioCoroutine").Value != null)
                        {
                            StopCoroutine(fieldTraverser.Field<Coroutine>("fadeBulletAudioCoroutine").Value);
                        }
                        fieldTraverser.Field<Coroutine>("fadeBulletAudioCoroutine").Value = StartCoroutine(FadeBulletAudio());
                        turret.bulletParticles.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
                        turret.rotationSpeed = 28f;
                        fieldTraverser.Field<bool>("rotatingSmoothly").Value = true;
                        turret.turretAnimator.SetInteger("TurretMode", 0);
                        fieldTraverser.Field<float>("turretInterval").Value = Random.Range(0f, 0.15f);
                    }

                    if (!IsServer)
                    {
                        break;
                    }

                    if (fieldTraverser.Field<float>("switchRotationTimer").Value >= 7f)
                    {
                        fieldTraverser.Field<float>("switchRotationTimer").Value = 0f;
                        bool setRotateRight = !fieldTraverser.Field<bool>("rotatingRight").Value;
                        turret.SwitchRotationClientRpc(setRotateRight);
                        turret.SwitchRotationOnInterval(setRotateRight);
                    }
                    else
                    {
                        fieldTraverser.Field<float>("switchRotationTimer").Value += Time.deltaTime;
                    }

                    if (fieldTraverser.Field<float>("turretInterval").Value >= 0.25f)
                    {
                        fieldTraverser.Field<float>("turretInterval").Value = 0f;
                        PlayerControllerB playerControllerB = turret.CheckForPlayersInLineOfSight(1.35f, angleRangeCheck: true);
                        EnemyAI enemy = CheckForEnemyInSight(1.35f);
                        if (enemy != null)
                        {
                            targetEnemy = enemy;
                            SwitchTurretMode(1);
                            SwitchTargetedEnemyClientRpc(enemy.NetworkObjectId, setModeToCharging: true);
                        }
                        else if (playerControllerB != null && !playerControllerB.isPlayerDead)
                        {
                            turret.targetPlayerWithRotation = playerControllerB;
                            SwitchTurretMode(1);
                            turret.SwitchTargetedPlayerClientRpc((int)playerControllerB.playerClientId, setModeToCharging: true);
                        }


                    }
                    else
                    {
                        fieldTraverser.Field<float>("turretInterval").Value += Time.deltaTime;
                    }

                    break;
                case TurretMode.Charging:
                    if (fieldTraverser.Field<TurretMode>("turretModeLastFrame").Value != TurretMode.Charging)
                    {
                        fieldTraverser.Field<TurretMode>("turretModeLastFrame").Value = TurretMode.Charging;
                        fieldTraverser.Field<bool>("rotatingClockwise").Value = false;
                        turret.mainAudio.PlayOneShot(turret.detectPlayerSFX);
                        turret.berserkAudio.Stop();
                        WalkieTalkie.TransmitOneShotAudio(turret.mainAudio, turret.detectPlayerSFX);
                        fieldTraverser.Field<float>("rotationSpeed").Value = 95f;
                        fieldTraverser.Field<bool>("rotatingSmoothly").Value = false;
                        fieldTraverser.Field<float>("lostLOSTimer").Value = 0f;
                        turret.turretAnimator.SetInteger("TurretMode", 1);
                    }

                    if (!IsServer)
                    {
                        break;
                    }

                    if (fieldTraverser.Field<float>("turretInterval").Value >= 1.5f)
                    {
                        fieldTraverser.Field<float>("turretInterval").Value = 0f;
                        if (!fieldTraverser.Field<bool>("hasLineOfSight").Value && turret.targetPlayerWithRotation != null)
                        {
                            turret.targetPlayerWithRotation = null;
                            turret.RemoveTargetedPlayerClientRpc();
                        }
                        if (!fieldTraverser.Field<bool>("hasLineOfSight").Value)
                        {
                            targetEnemy = null;
                            RemoveTargetedEnemyClientRpc();
                        }
                        else
                        {
                            SwitchTurretMode(2);
                            turret.SetToModeClientRpc(2);
                        }
                    }
                    else
                    {
                        fieldTraverser.Field<float>("turretInterval").Value += Time.deltaTime;
                    }

                    break;
                case TurretMode.Firing:
                    if (fieldTraverser.Field<TurretMode>("turretModeLastFrame").Value != TurretMode.Firing)
                    {
                        fieldTraverser.Field<TurretMode>("turretModeLastFrame").Value = TurretMode.Firing;
                        turret.berserkAudio.Stop();
                        turret.mainAudio.clip = turret.firingSFX;
                        turret.mainAudio.Play();
                        turret.farAudio.clip = turret.firingFarSFX;
                        turret.farAudio.Play();
                        turret.bulletParticles.Play(withChildren: true);
                        turret.bulletCollisionAudio.Play();
                        if (fieldTraverser.Field<Coroutine>("fadeBulletAudioCoroutine").Value != null)
                        {
                            StopCoroutine(fieldTraverser.Field<Coroutine>("fadeBulletAudioCoroutine").Value);
                        }

                        turret.bulletCollisionAudio.volume = 1f;
                        fieldTraverser.Field<bool>("rotatingSmoothly").Value = false;
                        fieldTraverser.Field<float>("lostLOSTimer").Value = 0f;
                        turret.turretAnimator.SetInteger("TurretMode", 2);
                    }

                    if (fieldTraverser.Field<float>("turretInterval").Value >= 0.21f)
                    {
                        fieldTraverser.Field<float>("turretInterval").Value = 0f;
                        DamagePlayerIfInSight();
                        DamageEnemyIfInSight();

                        fieldTraverser.Field<Ray>("shootRay").Value = new Ray(turret.aimPoint.position, turret.aimPoint.forward);
                        if (Physics.Raycast(fieldTraverser.Field<Ray>("shootRay").Value, out hit, 30f, StartOfRound.Instance.collidersAndRoomMask, QueryTriggerInteraction.Ignore))
                        {
                            turret.bulletCollisionAudio.transform.position = fieldTraverser.Field<Ray>("shootRay").Value.GetPoint(hit.distance - 0.5f);
                        }
                    }
                    else
                    {
                        fieldTraverser.Field<float>("turretInterval").Value += Time.deltaTime;
                    }

                    break;
                case TurretMode.Berserk:
                    if (fieldTraverser.Field<TurretMode>("turretModeLastFrame").Value != TurretMode.Berserk)
                    {
                        fieldTraverser.Field<TurretMode>("turretModeLastFrame").Value = TurretMode.Berserk;
                        turret.turretAnimator.SetInteger("TurretMode", 1);
                        fieldTraverser.Field<float>("berserkTimer").Value = 1.3f;
                        turret.berserkAudio.Play();
                        fieldTraverser.Field<float>("rotationSpeed").Value = 77f;
                        fieldTraverser.Field<bool>("enteringBerserkMode").Value = true;
                        fieldTraverser.Field<bool>("rotatingSmoothly").Value = true;
                        fieldTraverser.Field<float>("lostLOSTimer").Value = 0f;
                        fieldTraverser.Field<bool>("wasTargetingPlayerLastFrame").Value = false;
                        turret.targetPlayerWithRotation = null;
                        wasTargetingEnemyLastFrame = false;
                        targetEnemy = null;
                    }

                    if (fieldTraverser.Field<bool>("enteringBerserkMode").Value)
                    {
                        fieldTraverser.Field<float>("berserkTimer").Value -= Time.deltaTime;
                        if (fieldTraverser.Field<float>("berserkTimer").Value <= 0f)
                        {
                            fieldTraverser.Field<bool>("enteringBerserkMode").Value = false;
                            fieldTraverser.Field<bool>("rotatingClockwise").Value = true;
                            fieldTraverser.Field<float>("berserkTimer").Value = 9f;
                            turret.turretAnimator.SetInteger("TurretMode", 2);
                            turret.mainAudio.clip = turret.firingSFX;
                            turret.mainAudio.Play();
                            turret.farAudio.clip = turret.firingFarSFX;
                            turret.farAudio.Play();
                            turret.bulletParticles.Play(withChildren: true);
                            turret.bulletCollisionAudio.Play();
                            if (fieldTraverser.Field<Coroutine>("fadeBulletAudioCoroutine").Value != null)
                            {
                                StopCoroutine(fieldTraverser.Field<Coroutine>("fadeBulletAudioCoroutine").Value);
                            }

                            turret.bulletCollisionAudio.volume = 1f;
                        }

                        break;
                    }

                    if (fieldTraverser.Field<float>("turretInterval").Value >= 0.21f)
                    {
                        fieldTraverser.Field<float>("turretInterval").Value = 0f;
                        DamagePlayerIfInSight();
                        DamageEnemyIfInSight();

                        fieldTraverser.Field<Ray>("shootRay").Value = new Ray(turret.aimPoint.position, turret.aimPoint.forward);
                        if (Physics.Raycast(fieldTraverser.Field<Ray>("shootRay").Value, out hit, 30f, StartOfRound.Instance.collidersAndRoomMask, QueryTriggerInteraction.Ignore))
                        {
                            turret.bulletCollisionAudio.transform.position = fieldTraverser.Field<Ray>("shootRay").Value.GetPoint(hit.distance - 0.5f);
                        }
                    }
                    else
                    {
                        fieldTraverser.Field<float>("turretInterval").Value += Time.deltaTime;
                    }

                    if (IsServer)
                    {
                        fieldTraverser.Field<float>("berserkTimer").Value -= Time.deltaTime;
                        if (fieldTraverser.Field<float>("berserkTimer").Value <= 0f)
                        {
                            SwitchTurretMode(0);
                            turret.SetToModeClientRpc(0);
                        }
                    }

                    break;
            }

            if (fieldTraverser.Field<bool>("rotatingClockwise").Value)
            {
                turret.turnTowardsObjectCompass.localEulerAngles = new Vector3(-180f, turret.turretRod.localEulerAngles.y - Time.deltaTime * 20f, 180f);
                turret.turretRod.rotation = Quaternion.RotateTowards(turret.turretRod.rotation, turret.turnTowardsObjectCompass.rotation, turret.rotationSpeed * Time.deltaTime);
                return;
            }

            if (fieldTraverser.Field<bool>("rotatingSmoothly").Value)
            {
                turret.turnTowardsObjectCompass.localEulerAngles = new Vector3(-180f, Mathf.Clamp(turret.targetRotation, 0f - turret.rotationRange, turret.rotationRange), 180f);
            }

            turret.turretRod.rotation = Quaternion.RotateTowards(turret.turretRod.rotation, turret.turnTowardsObjectCompass.rotation, turret.rotationSpeed * Time.deltaTime);
        }
        [ServerRpc]
        public void SpawnTurretServerRpc(ulong turretId)
        {
            SpawnTurretClientRpc(turretId);
        }

        [ClientRpc]
        public void SpawnTurretClientRpc(ulong turretId)
        {
            turret = NetworkManager.Singleton.SpawnManager.SpawnedObjects[turretId].gameObject.GetComponentInChildren<Turret>();
            AndysModsPlugin.Log.LogInfo($"Lethal Turrets: spawning lethal turret for client {GameNetworkManager.Instance.localPlayerController?.playerUsername}, turret ID: {turret?.NetworkObjectId}.");
            HookupTurret(turret);
        }


        [ClientRpc]
        private void RemoveTargetedEnemyClientRpc()
        {
            targetEnemy = null;
        }

        [ClientRpc]
        private void SwitchTargetedEnemyClientRpc(ulong networkObjectId, bool setModeToCharging = false)
        {
            targetEnemy = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId].GetComponent<EnemyAI>();
            if (setModeToCharging)
            {
                SwitchTurretMode(1);
            }
        }

        private void DamageEnemyIfInSight()
        {
            if (CheckForEnemyInSight(3f) == targetEnemy && targetEnemy != null && !targetEnemy.isEnemyDead)
            {
                int dealtDamage = hit.distance >= 3f ? 1 : 2;
                targetEnemy.HitEnemy(dealtDamage, null, false);
            }
        }

        private void DamagePlayerIfInSight()
        {
            if (turret.CheckForPlayersInLineOfSight(3f) == GameNetworkManager.Instance.localPlayerController)
            {
                if (GameNetworkManager.Instance.localPlayerController.health > 50)
                {
                    GameNetworkManager.Instance.localPlayerController.DamagePlayer(50, hasDamageSFX: true, callRPC: true, CauseOfDeath.Gunshots);
                }
                else
                {
                    GameNetworkManager.Instance.localPlayerController.KillPlayer(turret.aimPoint.forward * 40f, spawnBody: true, CauseOfDeath.Gunshots);
                }
            }
        }
    }
}
