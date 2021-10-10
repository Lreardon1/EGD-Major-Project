using UnityEngine;

public class OneOnAttackAction : OneOnAction
{
    public OneOnTurnActor.Element element = OneOnTurnActor.Element.None;
    public int baseDamage = 1;

    public OneOnAttackAction(OneOnTurnActor.Element attackElement)
    {
        this.element = attackElement;
    }

    public override void TakeAction(OneOnTurnActor actor)
    {
        foreach (OneOnTurnActor t in targets)
        {
            int damage = baseDamage;
            if (OneOnTurnActor.ElementBeats(element, t.appliedElement))
            {
                Debug.Log(actor.name + " will do bonus damage because " + t.name + " has " + t.appliedElement);
                damage *= 2;
            }
            else if (OneOnTurnActor.ElementLosesTo(element, t.appliedElement))
            {
                Debug.Log(actor.name + " will do less damage because " + t.name + " has " + t.appliedElement);
                damage /= 2;
            }

            Debug.Log(actor.name + " attacks " + targets[0] + " for " + damage 
                + (element == OneOnTurnActor.Element.None ? "." : (" and applied " + element + ".")));

            if (element != OneOnTurnActor.Element.None)
                t.appliedElement = element;
            t.health -= damage;
            t.CheckIfNewlyDefeated();
        }
    }

    public override string ToString()
    {
        string s = "ATTACK (" + element.ToString() + ") -> ";
        for (int i = 0; i < targets.Length; ++i)
        {
            s += targets[i].name + ((i + 1 != targets.Length) ? ", " : ".");
        }
        return s;
    }
}