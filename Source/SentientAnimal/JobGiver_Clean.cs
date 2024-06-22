using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace SentientAnimals
{
    public class JobGiver_Clean : ThinkNode_JobGiver
    {
        private readonly int MinTicksSinceThickened = 600;
        public PathEndMode PathEndMode => PathEndMode.Touch;
        public ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Filth);
        public int MaxRegionsToScanBeforeGlobalSearch => 4;

        public IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.listerFilthInHomeArea.FilthInHomeArea;
        }

        public bool ShouldSkip(Pawn pawn)
        {
            return pawn.Map.listerFilthInHomeArea.FilthInHomeArea.Count == 0;
        }

        public bool HasJobOnThing(Pawn pawn, Thing t)
        {
            if (!(t is Filth filth))
            {
                return false;
            }

            if (!filth.Map.areaManager.Home[filth.Position])
            {
                return false;
            }

            if (!pawn.CanReserve(t))
            {
                return false;
            }

            if (filth.TicksSinceThickened < MinTicksSinceThickened)
            {
                return false;
            }

            return true;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!SentientAnimalsMod.settings.enableCleaningJobForSentient || ShouldSkip(pawn))
            {
                return null;
            }

            bool Predicate(Thing x)
            {
                return x.def.category == ThingCategory.Filth && HasJobOnThing(pawn, x);
            }

            var t = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.Filth),
                PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Some), 100f, Predicate, PotentialWorkThingsGlobal(pawn));
            if (t is null)
            {
                return null;
            }

            var job = JobMaker.MakeJob(JobDefOf.Clean);
            job.AddQueuedTarget(TargetIndex.A, t);
            var num = 15;
            var map = t.Map;
            var room = t.GetRoom();
            for (var i = 0; i < 100; i++)
            {
                var c2 = t.Position + GenRadial.RadialPattern[i];
                if (!ShouldClean(c2))
                {
                    continue;
                }

                var thingList = c2.GetThingList(map);
                foreach (var thing in thingList)
                {
                    if (HasJobOnThing(pawn, thing) && thing != t)
                    {
                        job.AddQueuedTarget(TargetIndex.A, thing);
                    }
                }

                if (job.GetTargetQueue(TargetIndex.A).Count >= num)
                {
                    break;
                }
            }

            if (job.targetQueueA.Count >= 5)
            {
                job.targetQueueA.SortBy(targ => targ.Cell.DistanceToSquared(pawn.Position));
            }

            return job;

            bool ShouldClean(IntVec3 c)
            {
                if (!c.InBounds(map))
                {
                    return false;
                }

                var room2 = c.GetRoom(map);
                if (room == room2)
                {
                    return true;
                }

                var region = c.GetDoor(map)?.GetRegion(RegionType.Portal);
                if (region == null || region.links.NullOrEmpty())
                {
                    return false;
                }

                foreach (var regionLink in region.links)
                {
                    for (var l = 0; l < 2; l++)
                    {
                        if (regionLink.regions[l] != null && !Equals(regionLink.regions[l], region) &&
                            regionLink.regions[l].valid && regionLink.regions[l].Room == room)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }
    }
};

