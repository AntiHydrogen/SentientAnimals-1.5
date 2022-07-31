using Verse;
using Verse.AI;

namespace SentientAnimals;

public class ThinkNode_SentientAnimal : ThinkNode_Conditional
{
    protected override bool Satisfied(Pawn pawn)
    {
        return pawn.IsSentient();
    }
}