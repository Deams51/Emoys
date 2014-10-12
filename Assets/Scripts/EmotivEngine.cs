using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; //This allows the IComparable Interface

public class EmotivEngine : MonoBehaviour
{
    public EmotionalCharacter character;
    private Pad emotionTransfer = new Pad(0,0,0);
    public double o;
    public double c;
    public double e;
    public double a;
    public double n;

    List<EmotionalCharacter.Action> actions;
    List<EmotionalCharacter.Consequence> conseqs;

    // Use this for initialization
    void Start()
    {
        character = new EmotionalCharacter(o, c, e, a, n);
        actions = new List<EmotionalCharacter.Action>();
        conseqs = new List<EmotionalCharacter.Consequence>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject o = collision.gameObject;
        EmotionObject obj = o.GetComponent("EmotionObject") as EmotionObject;
        if (obj != null)
        {
            EmotionalCharacter.Action a = new EmotionalCharacter.Action();
            a.Praisworthiness = obj.prais;
            a.Relationship = obj.relationship;
            actions.Add(a);
            EmotionalCharacter.Consequence c = new EmotionalCharacter.Consequence();
            c.Desirability = obj.desirability;
            c.Relationship = 0;
            actions.Add(a);
        }
    }

    void FixedUpdate()
    {
        //Debug.Log(character.mood);
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Emotion");
        List<EmotionalCharacter.ObjectEnv> objs = new List<EmotionalCharacter.ObjectEnv>();
        foreach (GameObject g in gos)
        {
            if ( (g.transform.position - this.transform.position).sqrMagnitude < 5)
            {
                EmotionalCharacter.ObjectEnv o = new EmotionalCharacter.ObjectEnv();
                o.Appealing = (g.GetComponent("EmotionObject") as EmotionObject).appeal;
                objs.Add(o);
                //13 0 3
                Debug.Log("appeal = " + o.Appealing);
            }
        }
        if (actions.Count > 0 || conseqs.Count > 0)
        {
            character.ProcessEvent(actions, conseqs, objs);
        }
        else character.NoEvent();
        actions.Clear();
        conseqs.Clear();
        double p = character._moodState.Pleasure;
        float s = (float)(2.0f * (p + 1.5f));


        this.transform.localScale = new Vector3(s,s,s);
        this.renderer.material.color = new Color((float)character._moodState.Pleasure, (float)character._moodState.Arousal, (float)character._moodState.Dominance);
        //switch (emotionTransfer.Name)
        //{
        //    case "Admiration":
        //        break;
        //    case "Anger":
        //        break;
        //    case "Distress":
        //        break;
        //    case "Fear":
        //        //var actions = new List<EmotionalCharacter.Action>();
        //        //EmotionalCharacter.Action a1 = new EmotionalCharacter.Action();
        //        //a1.Praisworthiness = -1;
        //        //a1.Relationship =0;
        //        //actions.Add(a1);
        //        //var conseqs = new List<EmotionalCharacter.Consequence>();
        //        //EmotionalCharacter.Consequence c1 = new EmotionalCharacter.Consequence();
        //        //c1.Desirability = -1;
        //        //c1.Relationship = 0;
        //        //conseqs.Add(c1);
        //        //EmotionalCharacter.Consequence c2 = new EmotionalCharacter.Consequence();
        //        //c2.Desirability = 1;
        //        //c2.Relationship = -1;
        //        //conseqs.Add(c2);
        //        //character.ProcessEvent(actions, conseqs, new List<EmotionalCharacter.ObjectEnv>());
        //        break;
        //    case "Gloating":
        //        break;
        //    case "Gratification":
        //        break;
        //    case "Gratitude":
        //        break;
        //    case "Happyfor":
        //        break;
        //    case "Hate":
        //        break;
        //    case "Joy":
        //        break;
        //    case "Love":
        //        break;
        //    case "Pity":
        //        break;
        //    case "Pride":
        //        break;
        //    case "Remorse":
        //        break;
        //    case "Reproach":
        //        break;
        //    case "Resentment":
        //        break;
        //    case "Shame":
        //        break;
        //}
    }

    void OnGUI()
    {
    }
}

public class EmotionalCharacter
{
    public Pad _moodState = new Pad(0, 0, 0);
    public Pad _originalMoodState = new Pad(0, 0, 0);
    public string mood = "";

    public struct Action
    {
        public double Praisworthiness;
        public double Relationship;
    };

    public struct Consequence
    {
        public double Desirability;
        public double Relationship;
    };

    public struct ObjectEnv
    {
        public double Appealing;
    };

    public EmotionalCharacter(double o, double c, double e, double a, double n)
    {
        _originalMoodState = new Pad(o, c, e, a, n, "OCEAN - Init");
        _moodState = _originalMoodState; 
        Pad closest = find_closest(_moodState);
        mood = closest.Name;
    }

    public void NoEvent()
    {
        // Attenuation function
        // check moodstate with reference point
        _moodState += (0.001*(_originalMoodState + (-1*_moodState)));

        Pad closest = find_closest(_moodState);
        Debug.Log("Mood noevent - P: " + _moodState.Pleasure + ", A: " + _moodState.Arousal + ", D: " + _moodState.Dominance);
        Debug.Log("Mood noevent: " + closest.Name);
        GameObject[] gos = GameObject.FindGameObjectsWithTag("EmotionText");
        gos[0].guiText.text = "Current Emotion: " + closest.Name;
    }
    // Check the scaling parameters describd in paper, seems P,R are used for actions and R for attributes
    public string ProcessEvent(List<Action> actions, List<Consequence> consequences, List<ObjectEnv> objects)
    {
        List<Pad> actionEmot = new List<Pad>();
        List<Pad> ConseqEmot = new List<Pad>();
        Pad sum = new Pad(0, 0, 0);

        foreach (Action action in actions)
        {
            Pad temp = ActionEvaluation(action.Praisworthiness, action.Relationship);
            actionEmot.Add(temp);
            sum += temp;
        }
        foreach (Consequence c in consequences)
        {
            Pad temp = ConsequenceEvaluation(c.Desirability, c.Relationship);
            ConseqEmot.Add(temp);
            sum += temp;
        }
        foreach (Pad c in ConseqEmot)
        {
            foreach (Pad a in actionEmot)
            {
                sum += AttributeEmotionEvaluation(a, c);
            }
        }
        foreach (ObjectEnv o in objects)
        {
            Pad test = ObjectEmotion(o.Appealing);
            Debug.Log("test - P: " + test.Pleasure + ", A: " + test.Arousal + ", D: " + test.Dominance);

            sum += ObjectEmotion(o.Appealing);
        }
        // scaling
        sum.Normalize();
        sum = 0.1 * sum;

        _moodState += sum;

        _moodState.Normalize();

        //Closest label
        Pad closest = find_closest(_moodState);
        Debug.Log("Mood - P: " + _moodState.Pleasure + ", A: " + _moodState.Arousal + ", D: " + _moodState.Dominance);
        Debug.Log("Mood: " + closest.Name);
        GameObject[] gos = GameObject.FindGameObjectsWithTag("EmotionText");
        gos[0].guiText.text = "Current Emotion: " + closest.Name;
        mood = closest.Name;
        return closest.Name;
    }

    private Pad ActionEvaluation(double praisworthiness, double relationship)
    {
        // if self
        if (Math.Abs(relationship) < 0.01)
        {
            if (praisworthiness > 0) return Emotions.Pride;
            else return Emotions.Shame;
        }
        else
        {
            if (praisworthiness > 0) return Emotions.Admiration;
            else return Emotions.Reproach;
        }
    }

    private Pad ConsequenceEvaluation(double desirability, double relationship)
    {
        if (desirability > 0)
        {
            if (Math.Abs(relationship) < 0.01) return Emotions.Joy;
            else if (relationship > 0) return Emotions.Happyfor;
            else return Emotions.Resentment;
        }
        else
        {
            if (Math.Abs(relationship) < 0.01) return Emotions.Distress;
            else if (relationship > 0) return Emotions.Pity;
            else return Emotions.Gloating;
        }
    }

    private Pad AttributeEmotionEvaluation(Pad action, Pad consequence)
    {
        if (consequence == Emotions.Joy)
        {
            if (action == Emotions.Pride) return Emotions.Gratification;
            else if (action == Emotions.Admiration) return Emotions.Gratitude;
        }
        else if (consequence == Emotions.Distress)
        {
            if (action == Emotions.Shame) return Emotions.Remorse;
            else if (action == Emotions.Reproach) return Emotions.Anger;
        }
        return new Pad(0, 0, 0, "Empty attribute emotion");
    }

    private Pad ObjectEmotion(double appealing)
    {
        if (appealing > 0) return Emotions.Love;
        else if (appealing < 0) return Emotions.Hate;
        return new Pad(0, 0, 0, "Empty object emotion");
    }

    private Pad find_closest(Pad p)
    {
        Pad closest = null;
        double distanceMin = 0, distance = 0;
        foreach (Pad es in Emotions.References)
        {
            distance = es.DistanceTo(p);
            //Debug.Log("Distance from mood to " + es.Name + ": " + distance);
            if (closest == null || distance < distanceMin)
            {
                distanceMin = distance;
                closest = es;
            }
        }
        return closest;
    }
}

public static class Emotions
{
    // Emotional Mapping
    public static readonly Pad Admiration = new Pad(0.5, 0.3, -0.2, "Admiration");
    public static readonly Pad Anger = new Pad(-0.51, 0.59, 0.25, "Anger");
    public static readonly Pad Distress = new Pad(-0.4, -0.2, -0.5, "Distress");
    public static readonly Pad Fear = new Pad(-0.6, 0.6, -0.4, "Fear");
    public static readonly Pad Gloating = new Pad(0.3, -0.3, -0.1, "Gloating");
    public static readonly Pad Gratification = new Pad(0.6, 0.5, 0.4, "Gratification");
    public static readonly Pad Gratitude = new Pad(0.4, 0.2, -0.3, "Gratitude");
    public static readonly Pad Happyfor = new Pad(0.4, 0.2, 0.2, "Happyfor");
    public static readonly Pad Hate = new Pad(-0.6, 0.6, 0.3, "Hate");
    public static readonly Pad Joy = new Pad(0.4, 0.2, 0.1, "Joy");
    public static readonly Pad Love = new Pad(0.3, 0.1, 0.2, "Love");
    public static readonly Pad Pity = new Pad(-0.4, -0.2, -0.5, "Pity");
    public static readonly Pad Pride = new Pad(0.4, 0.3, 0.3, "Pride");
    public static readonly Pad Remorse = new Pad(-0.3, 0.1, -0.6, "Remorse");
    public static readonly Pad Reproach = new Pad(-0.3, -0.1, 0.4, "Reproach");
    public static readonly Pad Resentment = new Pad(-0.2, -0.3, -0.2, "Resentment");
    public static readonly Pad Shame = new Pad(-0.3, 0.1, -0.6, "Shame");

    //// Reference points
    //public static readonly List<Pad> References = new List<Pad>
    //{
    //    new Pad(0.2,-0.1,0.3,"Normal"),
    //    new Pad(-0.4,0.6,-0.5,"Afraid"),
    //    new Pad(-0.2,0.8,0.8,"Angry")
    //};

    // Reference points
    public static readonly List<Pad> References = new List<Pad>
        {
            new Pad(0.5, 0.3, -0.2, "Admiration"),
            new Pad(-0.51, 0.59, 0.25, "Anger"),
            new Pad(-0.4, -0.2, -0.5, "Distress"),
            new Pad(-0.6, 0.6, -0.4, "Fear"),
            new Pad(0.3, -0.3, -0.1, "Gloating"),
            new Pad(0.6, 0.5, 0.4, "Gratification"),
            new Pad(0.4, 0.2, -0.3, "Gratitude"),
            new Pad(0.4, 0.2, 0.2, "Happyfor"),
            new Pad(-0.6, 0.6, 0.3, "Hate"),
            new Pad(0.4, 0.2, 0.1, "Joy"),
            new Pad(0.3, 0.1, 0.2, "Love"),
            new Pad(-0.4, -0.2, -0.5, "Pity"),
            new Pad(0.4, 0.3, 0.3, "Pride"),
            new Pad(-0.3, 0.1, -0.6, "Remorse"),
            new Pad(-0.3, -0.1, 0.4, "Reproach"),
            new Pad(-0.2, -0.3, -0.2, "Resentment"),
            new Pad(0.3, -0.2, 0.4, "Satisfaction"),
            new Pad(-0.3, 0.1, -0.6, "Shame")
        };
}
public class Pad
{
    public Pad(double pleasure, double arousal, double dominance, string name = "")
    {
        Pleasure = pleasure;
        Arousal = arousal;
        Dominance = dominance;
        Name = name;
    }

    public Pad(double openness, double conscientiousness, double extraversion, double agreeableness,
        double neuroticism, string name = "")
    {
        Pleasure = 0.21 * extraversion + 0.59 * agreeableness + 0.19 * neuroticism;
        Arousal = 0.15 * openness + 0.30 * agreeableness + 0.57 * neuroticism;
        Dominance = 0.25 * openness + 0.17 * conscientiousness + 0.6 * extraversion - 0.32 * agreeableness;
        Name = name;
    }

    public Pad(Pad pad)
    {
        Pleasure = pad.Pleasure;
        Arousal = pad.Arousal;
        Dominance = pad.Dominance;
        Name = pad.Name;
    }

    public double Pleasure { get; set; }
    public double Arousal { get; set; }
    public double Dominance { get; set; }
    public string Name { get; set; }

    public void Normalize()
    {
        double distance = DistanceTo(new Pad(0, 0, 0));
        if (Math.Abs(distance) > 0.01)
        {
            Pleasure /= distance;
            Arousal /= distance;
            Dominance /= distance;
        }
    }

    public double DistanceTo(Pad p)
    {
        return Math.Sqrt((p.Pleasure - Pleasure) * (p.Pleasure - Pleasure) +
                         (p.Arousal - Arousal) * (p.Arousal - Arousal) +
                         (p.Dominance - Dominance) * (p.Dominance - Dominance));
    }

    public static Pad operator +(Pad p1, Pad p2)
    {
        return new Pad(p1.Pleasure + p2.Pleasure, p1.Arousal + p2.Arousal, p1.Dominance + p2.Dominance);
    }

    public static Pad operator *(double s, Pad p1)
    {
        return new Pad(p1.Pleasure * s, p1.Arousal * s, p1.Dominance * s);
    }
}