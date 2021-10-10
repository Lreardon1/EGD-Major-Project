using UnityEngine;

public class OneOnPartyMember : OneOnTurnActor
{

    public override bool IsPlayerAlly()
    {
        return true;
    }

    public override void PickNextAction()
    {
        Element attackElement = Element.None;
        if (Random.value > 0.8f)
        {
            float choice = Random.value;
            if (choice > 0.75)
                attackElement = Element.Fire;
            else if (choice > 0.5)
                attackElement = Element.Water;
            else if (choice > 0.25)
                attackElement = Element.Gust;
            else
                attackElement = Element.Lightning;
        }

        nextAction = new OneOnAttackAction(attackElement);
        nextAction.SetTargets(manager.GetRandomEnemy());
        print(name + " picks new target of " + nextAction.targets[0].name);
    }

    public override void UpdateScene(OneOnCombatManager manager)
    {
        base.UpdateScene(manager);

        if (nextAction.GetType() == typeof(OneOnAttackAction) && nextAction.targets[0].GetIsDefeated())
        {
            nextAction.SetTargets(manager.GetRandomEnemy());
        }
    }
}
