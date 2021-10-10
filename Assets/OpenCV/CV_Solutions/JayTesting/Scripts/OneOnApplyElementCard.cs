using UnityEngine;

public class OneOnApplyElementCard : OneOnPlayCard
{
    private OneOnTurnActor.Element element;

    public OneOnApplyElementCard(OneOnTurnActor.Element element)
    {
        this.element = element;
    }

    public override void ModifyAction(OneOnAction action)
    {
        if (action.GetType() == typeof(OneOnAttackAction))
        {
            OneOnAttackAction atkAction = action as OneOnAttackAction;
            atkAction.element = element;
        }
    }
}