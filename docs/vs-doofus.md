## vs. Doofuses example
Svelto's GroupCompound is good enough for simple, static groups. But not all the groups in game is simple or static. Most of them are not, actually. Let's look at the Doofuses example of Svelto.ECS.MiniExamples. They have groups like this.
```csharp
static class GameGroups
{
    public class DOOFUSES : GroupTag<DOOFUSES> { }
    public class FOOD : GroupTag<FOOD> { }
    
    public class RED : GroupTag<RED> { }
    public class BLUE : GroupTag<BLUE> { }
    
    public class EATING : GroupTag<EATING> { }
    public class NOTEATING : GroupTag<NOTEATING> { }

    public class RED_DOOFUSES_EATING : GroupCompound<DOOFUSES, RED, EATING> { };
    public class RED_DOOFUSES_NOT_EATING :  GroupCompound<DOOFUSES, RED, NOTEATING> { };
    public class RED_FOOD_EATEN : GroupCompound<FOOD, RED, EATING> { };
    public class RED_FOOD_NOT_EATEN : GroupCompound<FOOD, RED, NOTEATING> { };
    
    public class BLUE_DOOFUSES_EATING : GroupCompound<DOOFUSES, BLUE, EATING> { };
    public class BLUE_DOOFUSES_NOT_EATING :  GroupCompound<DOOFUSES, BLUE, NOTEATING> { };
    public class BLUE_FOOD_EATEN : GroupCompound<FOOD, BLUE, EATING> { };
    public class BLUE_FOOD_NOT_EATEN : GroupCompound<FOOD, BLUE, NOTEATING> { };

    public class DOOFUSES_EATING : GroupCompound<DOOFUSES, EATING> { };
}
```
There is entity type of Doofuses and Food, team of Red and Blue, state of Eating and NonEating. And groups are made with their combinations. I think it will be easy if you get used to it, but little confusing to understand structure at the first.

Real problem is it is not really flexible nor extendible. What if Yellow team is needed? What if state of Flying and Ground is needed? We'll have to define all the combinations we need. Game design will change over time, and I think it is not managable through GroupCompound at some point. 

With Schema extension this would be converted to below.
```csharp
public class StateSchema : IEntitySchema
{
    public readonly Table<DoofusRow> Doofus = new Table<DoofusRow>();
    public readonly Table<FoodRow> Food = new Table<FoodRow>();
}

public class TeamSchema : IEntitySchema
{
    public readonly StateSchema Eating = new StateSchema();
    public readonly StateSchema NonEating = new StateSchema();
}

public class GameSchema : IEntitySchema
{
    public readonly Ranged<TeamSchema, TeamColor> Teams =
        new Ranged<TeamSchema, TeamColor>(TeamColor.MAX, color => (int)color);

    public readonly IEntityTables<DoofusRow> EatingDoofuses;

    public GameSchema()
    {
        EatingDoofuses = Teams.Combine(x => x.Eating.Doofus);
    }
}

public enum TeamColor { Red, Blue, MAX }
```
Now we can easly change structure without fixed names, and have changable number of teams. Type-safe queries are bonus. You'll thank to some complexity when you have to deal with big design changes!

When using it, code `GameGroups.RED_DOOFUSES_EATING.Groups` would be equvalent to `GameSchema.Teams[TeamColor.Red].Eating.Doofus`.
