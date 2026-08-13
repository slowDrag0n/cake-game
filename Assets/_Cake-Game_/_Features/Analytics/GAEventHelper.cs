using UnityEngine;
using GameAnalyticsSDK;

public class GAEventHelper : MonoBehaviour
{
    LevelSequence seq;

    private void Awake()
    {
        seq = GetComponent<LevelSequence>();
        if(seq == null)
            seq = GetComponentInParent<LevelSequence>();
    }

    public void LogLevelStartEvent(string levelName)
    {
        if(!canLog) return;

        //Debug.Log($" >>>>>>>>> Log GA Progression Status - Start, {levelName}");
        GAManager.Instance.LogProgressionEvent(GAProgressionStatus.Start, levelName);
    }

    public void LogLevelCompleteEvent(string levelName)
    {
        if(!canLog) return;

        //Debug.Log($" >>>>>>>>> Log GA Progression Status - Complete, {levelName}");
        GAManager.Instance.LogProgressionEvent(GAProgressionStatus.Complete, levelName);
    }

    public void LogSequenceStartEvent(string sequenceTask)
    {
        if(!canLog) return;

        var levelName = EventManager.DoFireGetLevelName();
        var seqId = seq.SequenceId;
        var eventString = $"{seqId}_{sequenceTask}";

        //Debug.Log($" >>>>>>>>> Log GA Progression Status - Start, {levelName}:{eventString}");
        GAManager.Instance.LogProgressionEvent(GAProgressionStatus.Start, levelName, eventString);
    }

    public void LogSequenceCompleteEvent(string sequenceTask)
    {
        if(!canLog) return;

        var levelName = EventManager.DoFireGetLevelName();
        var seqId = seq.SequenceId;
        var eventString = $"{seqId}_{sequenceTask}";

        //Debug.Log($" >>>>>>>>> Log GA Progression Status - Complete, {levelName}:{eventString}");
        GAManager.Instance.LogProgressionEvent(GAProgressionStatus.Complete, levelName, eventString);
    }


    //bool canLog => seq != null;
    bool canLog => true;
}
