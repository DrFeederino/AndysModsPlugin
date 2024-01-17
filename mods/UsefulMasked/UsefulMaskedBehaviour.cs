//using GameNetcodeStuff;
//using HarmonyLib;
//using System;
//using System.Reflection;
//using Unity.Netcode;
//using UnityEngine;
//using UnityEngine.AI;

//namespace AndysModsPlugin.mods.UsefulMasked
//{
//    internal class UsefulMaskedBehaviour : NetworkBehaviour
//    {
//        private AISearchRoutine searchForItems;
//        private MaskedPlayerEnemy masked;
//        private Traverse fieldTraverser;
//        private GrabbableObject[] ItemSlots = new GrabbableObject[4];
//        private GrabbableObject targetItem;
//        private GrabbableObject currentlyHeldItem;
//        private Transform grabTarget;
//        private bool isFullyLoadedAndGoingHome = false;

//        public void HookupMasked(MaskedPlayerEnemy masked)
//        {
//            this.masked = masked;
//            fieldTraverser = Traverse.Create(masked);
//            masked.enabled = false;
//        }


//        private void CalculateAnimationDirection()
//        {
//            MethodInfo dynMethod = masked.GetType().GetMethod("CalculateAnimationDirection", BindingFlags.NonPublic | BindingFlags.Instance);
//            dynMethod.Invoke(masked, []);
//        }

//        private void ChooseShipHidingSpot()
//        {
//            MethodInfo dynMethod = masked.GetType().GetMethod("ChooseShipHidingSpot", BindingFlags.NonPublic | BindingFlags.Instance);
//            dynMethod.Invoke(masked, []);
//        }

//        private void CheckIfAtHome()
//        {
//            AndysModsPlugin.Log.LogInfo("Dropping items.");

//            ItemSlots.Do(item =>
//            {
//                AndysModsPlugin.Log.LogInfo($"Dropping item {item.name}.");
//                currentlyHeldItem = item;
//                DropItem();
//            });
//        }

//        public void Update()
//        {
//            masked.SetVisibilityOfMaskedEnemy();
//            if (masked.isEnemyDead)
//            {
//                masked.agent.speed = 0f;
//                if (masked.inSpecialAnimation)
//                {
//                    masked.FinishKillAnimation();
//                }

//                return;
//            }
//            if (fieldTraverser.Field<PlayerControllerB>("lastPlayerKilled").Value != null && fieldTraverser.Field<PlayerControllerB>("lastPlayerKilled").Value.deadBody != null && !fieldTraverser.Field<PlayerControllerB>("lastPlayerKilled").Value.deadBody.deactivated)
//            {
//                fieldTraverser.Field<PlayerControllerB>("lastPlayerKilled").Value.deadBody.DeactivateBody(setActive: false);
//                fieldTraverser.Field<PlayerControllerB>("lastPlayerKilled").Value = null;
//            }

//            if (!fieldTraverser.Field<bool>("enemyEnabled").Value)
//            {
//                return;
//            }
//            if (isFullyLoadedAndGoingHome)
//            {
//                CheckIfAtHome();
//            }
//            if (!masked.ventAnimationFinished)
//            {
//                masked.lookRig1.weight = 0f;
//                masked.lookRig2.weight = 0f;
//                return;
//            }
//            masked.lookRig1.weight = 0.452f;
//            masked.lookRig2.weight = 1f;
//            masked.creatureAnimator.SetBool("Stunned", masked.stunNormalizedTimer >= 0f);
//            if (masked.stunNormalizedTimer >= 0f)
//            {
//                masked.agent.speed = 0f;
//                if (masked.IsOwner && masked.searchForPlayers.inProgress)
//                {
//                    masked.StopSearch(masked.searchForPlayers);
//                }

//                if (masked.inSpecialAnimation)
//                {
//                    masked.FinishKillAnimation();
//                }
//                if (masked.IsOwner && searchForItems.inProgress)
//                {
//                    masked.StopSearch(searchForItems);
//                }
//            }
//            else
//            {
//                if (masked.inSpecialAnimation)
//                {
//                    return;
//                }

//                if (masked.walkCheckInterval <= 0f)
//                {
//                    masked.walkCheckInterval = 0.1f;
//                    fieldTraverser.Field<Vector3>("positionLastCheck").Value = masked.transform.position;
//                }
//                else
//                {
//                    masked.walkCheckInterval -= Time.deltaTime;
//                }

//                switch (masked.currentBehaviourStateIndex)
//                {
//                    case 0:

//                        if (fieldTraverser.Field<int>("previousBehaviourState").Value != masked.currentBehaviourStateIndex)
//                        {
//                            masked.stareAtTransform = null;
//                            fieldTraverser.Field<bool>("running").Value = false;
//                            fieldTraverser.Field<bool>("runningRandomly").Value = false;
//                            masked.creatureAnimator.SetBool("Running", value: false);

//                            fieldTraverser.Field<bool>("handsOut").Value = false;
//                            masked.creatureAnimator.SetBool("HandsOut", value: false);
//                            fieldTraverser.Field<bool>("crouching").Value = false;
//                            masked.creatureAnimator.SetBool("Crouching", value: false);
//                            fieldTraverser.Field<int>("previousBehaviourState").Value = masked.currentBehaviourStateIndex;
//                        }
//                        if (GrabTargetItemIfClose())
//                        {
//                            break;
//                        }

//                        if (fieldTraverser.Field<bool>("running").Value || fieldTraverser.Field<bool>("runningRandomly").Value)
//                        {
//                            masked.agent.speed = 7f;
//                        }
//                        else
//                        {
//                            masked.agent.speed = 3.8f;
//                        }

//                        break;
//                    case 1:
//                        if (fieldTraverser.Field<int>("previousBehaviourState").Value != masked.currentBehaviourStateIndex)
//                        {

//                            fieldTraverser.Field<float>("lookAtPositionTimer").Value = 0f;
//                            if (fieldTraverser.Field<int>("previousBehaviourState").Value == 0)
//                            {
//                                fieldTraverser.Field<float>("stopAndStareTimer").Value = UnityEngine.Random.Range(2f, 5f);
//                            }

//                            fieldTraverser.Field<bool>("runningRandomly").Value = false;
//                            fieldTraverser.Field<bool>("running").Value = false;
//                            masked.creatureAnimator.SetBool("Running", value: false);
//                            fieldTraverser.Field<bool>("crouching").Value = false;
//                            masked.creatureAnimator.SetBool("Crouching", value: false);
//                            fieldTraverser.Field<int>("previousBehaviourState").Value = masked.currentBehaviourStateIndex;
//                        }

//                        if (!masked.IsOwner)
//                        {
//                            break;
//                        }

//                        fieldTraverser.Field<float>("stopAndStareTimer").Value -= Time.deltaTime;
//                        if (fieldTraverser.Field<float>("stopAndStareTimer").Value >= 0f)
//                        {
//                            masked.agent.speed = 0f;
//                            break;
//                        }

//                        if (fieldTraverser.Field<float>("stopAndStareTimer").Value <= -5f)
//                        {
//                            fieldTraverser.Field<float>("stopAndStareTimer").Value = UnityEngine.Random.Range(0f, 3f);
//                        }

//                        if (fieldTraverser.Field<bool>("running").Value || fieldTraverser.Field<bool>("runningRandomly").Value)
//                        {
//                            masked.agent.speed = 8f;
//                        }
//                        else
//                        {
//                            masked.agent.speed = 3.8f;
//                        }

//                        break;
//                    case 2:
//                        if (fieldTraverser.Field<int>("previousBehaviourState").Value != masked.currentBehaviourStateIndex)
//                        {
//                            masked.movingTowardsTargetPlayer = false;
//                            fieldTraverser.Field<float>("interestInShipCooldown").Value = 17f;
//                            masked.agent.speed = 5f;
//                            fieldTraverser.Field<bool>("runningRandomly").Value = false;
//                            fieldTraverser.Field<bool>("running").Value = false;
//                            masked.creatureAnimator.SetBool("Running", value: false);
//                            fieldTraverser.Field<bool>("handsOut").Value = false;
//                            masked.creatureAnimator.SetBool("HandsOut", value: false);
//                            if (masked.IsOwner)
//                            {
//                                ChooseShipHidingSpot();
//                            }

//                            fieldTraverser.Field<int>("previousBehaviourState").Value = masked.currentBehaviourStateIndex;
//                        }
//                        GrabbableObject potentialItemPickup = CheckLineOfSightForItem(HoarderBugItemStatus.Returned, 60f, 12, 3f);
//                        if (potentialItemPickup != null && !potentialItemPickup.isHeld)
//                        {
//                            masked.currentBehaviourStateIndex = 0;
//                            SetGoTowardsTargetObject(potentialItemPickup.gameObject);
//                        }
//                        else
//                        {
//                            masked.currentBehaviourStateIndex = 1;
//                        }

//                        break;
//                }
//            }
//        }
//        private void SetGoTowardsTargetObject(GameObject foundObject)
//        {
//            if (SetDestinationToPosition(foundObject.transform.position, checkForPath: true) && HoarderBugAI.grabbableObjectsInMap.Contains(foundObject))
//            {
//                AndysModsPlugin.Log.LogInfo($"Lethal Masked: found an object, directing masked enemy to grab {gameObject.name}.");
//                targetItem = foundObject.GetComponent<GrabbableObject>();
//                masked.StopSearch(searchForItems, clear: false);
//            }
//            else
//            {
//                targetItem = null;
//                AndysModsPlugin.Log.LogInfo($"Lethal Masked: found an object, but it's unreachable {gameObject.name}.");
//            }
//        }

//        public bool SetDestinationToPosition(Vector3 position, bool checkForPath = false)
//        {
//            if (checkForPath)
//            {
//                position = RoundManager.Instance.GetNavMeshPosition(position, RoundManager.Instance.navHit, 1.75f);
//                masked.path1 = new NavMeshPath();
//                if (!masked.agent.CalculatePath(position, masked.path1))
//                {
//                    return false;
//                }

//                if (Vector3.Distance(masked.path1.corners[masked.path1.corners.Length - 1], RoundManager.Instance.GetNavMeshPosition(position, RoundManager.Instance.navHit, 2.7f)) > 1.55f)
//                {
//                    return false;
//                }
//            }

//            masked.moveTowardsDestination = true;
//            masked.movingTowardsTargetPlayer = false;
//            masked.destination = RoundManager.Instance.GetNavMeshPosition(position, RoundManager.Instance.navHit, -1f);
//            return true;
//        }

//        private bool GrabTargetItemIfClose()
//        {
//            AndysModsPlugin.Log.LogInfo($"Lethal Masked: picking up the item {targetItem.name}");
//            if (targetItem != null && currentlyHeldItem == null && Vector3.Distance(base.transform.position, targetItem.transform.position) < 0.75f)
//            {
//                NetworkObject networkObject = targetItem.GetComponent<NetworkObject>();
//                masked.SwitchToBehaviourStateOnLocalClient(1);
//                GrabItemServerRpc(networkObject);
//                return true;
//            }

//            return false;
//        }

//        [ServerRpc]
//        public void GrabItemServerRpc(NetworkObjectReference targetItem)
//        {
//            GrabItemClientRpc(targetItem);
//        }

//        [ClientRpc]
//        public void GrabItemClientRpc(NetworkObjectReference targetItem)
//        {
//            masked.SwitchToBehaviourStateOnLocalClient(1);
//            if (targetItem.TryGet(out var networkObject))
//            {
//                GrabItem(networkObject);
//            }
//        }

//        [ServerRpc]
//        public void DropItemServerRpc()
//        {
//            DropItemClientRpc();
//        }

//        [ClientRpc]
//        public void DropItemClientRpc()
//        {
//            DropItem();
//        }

//        private void DropItem()
//        {
//            if (currentlyHeldItem == null)
//            {
//                return;
//            }
//            currentlyHeldItem.parentObject = null;
//            currentlyHeldItem.transform.SetParent(StartOfRound.Instance.propsContainer, worldPositionStays: true);
//            currentlyHeldItem.EnablePhysics(enable: true);
//            currentlyHeldItem.fallTime = 0f;
//            currentlyHeldItem.startFallingPosition = currentlyHeldItem.transform.parent.InverseTransformPoint(currentlyHeldItem.transform.position);
//            currentlyHeldItem.targetFloorPosition = currentlyHeldItem.transform.parent.InverseTransformPoint(currentlyHeldItem.GetItemFloorPosition());
//            currentlyHeldItem.floorYRot = -1;
//            currentlyHeldItem.DiscardItemFromEnemy();
//            currentlyHeldItem = null;
//            HoarderBugAI.grabbableObjectsInMap.Add(currentlyHeldItem.gameObject);
//        }

//        private void GrabItem(NetworkObject item)
//        {
//            for (int i = 0; i < ItemSlots.Length; i++)
//            {
//                if (ItemSlots[i] == null)
//                {
//                    ItemSlots[i] = item.gameObject.GetComponent<GrabbableObject>();
//                    currentlyHeldItem.parentObject = grabTarget; // basically null?
//                    currentlyHeldItem.hasHitGround = false;
//                    currentlyHeldItem.GrabItemFromEnemy(masked);
//                    currentlyHeldItem.EnablePhysics(enable: false);
//                    HoarderBugAI.grabbableObjectsInMap.Remove(currentlyHeldItem.gameObject);
//                    return;
//                }
//            }
//            // This is only reachable if for some reason masked guy tries to pick up where inventory is full
//            AndysModsPlugin.Log.LogInfo("Lethal Masked: tried to pick up the item, but the masked is fully loaded. Time to go home.");
//            SetDestinationToPosition(StartOfRound.Instance.insideShipPositions[0].position);
//            isFullyLoadedAndGoingHome = true;
//        }


//        public GrabbableObject CheckLineOfSightForItem(HoarderBugItemStatus searchForItemsOfStatus = HoarderBugItemStatus.Any, float width = 45f, int range = 60, float proximityAwareness = -1f)
//        {
//            for (int i = 0; i < HoarderBugAI.HoarderBugItems.Count; i++)
//            {
//                if (!HoarderBugAI.HoarderBugItems[i].itemGrabbableObject.grabbableToEnemies || HoarderBugAI.HoarderBugItems[i].itemGrabbableObject.isHeld || (searchForItemsOfStatus != HoarderBugItemStatus.Any && HoarderBugAI.HoarderBugItems[i].status != searchForItemsOfStatus))
//                {
//                    continue;
//                }

//                Vector3 position = HoarderBugAI.HoarderBugItems[i].itemGrabbableObject.transform.position;
//                if (!Physics.Linecast(masked.eye.position, position, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
//                {
//                    Vector3 to = position - masked.eye.position;
//                    if (Vector3.Angle(masked.eye.forward, to) < width || Vector3.Distance(base.transform.position, position) < proximityAwareness)
//                    {
//                        return HoarderBugAI.HoarderBugItems[i].itemGrabbableObject;
//                    }
//                }
//            }

//            return null;
//        }


//        public override void OnNetworkSpawn()
//        {
//            AndysModsPlugin.Log.LogInfo("Useful Masked: spawned network object for masked enemy!");
//            base.OnNetworkSpawn();
//        }

//        [ServerRpc]
//        public void SpawnMaskedServerRpc(ulong maskedId)
//        {
//            SpawnMaskedClientRpc(maskedId);
//        }

//        [ClientRpc]
//        public void SpawnMaskedClientRpc(ulong maskedId)
//        {
//            masked = NetworkManager.Singleton.SpawnManager.SpawnedObjects[maskedId].gameObject.GetComponentInChildren<MaskedPlayerEnemy>();
//            AndysModsPlugin.Log.LogInfo($"Useful Masked: spawning useful masked enemy for client {GameNetworkManager.Instance.localPlayerController?.playerUsername}, masked ID: {masked?.NetworkObjectId}.");
//            HookupMasked(masked);
//        }

//    }
//}
