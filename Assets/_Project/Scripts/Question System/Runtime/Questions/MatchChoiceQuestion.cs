using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zone8.Question.Runtime.Base;

namespace Zone8.Question.Core
{
    [Serializable]
    public struct MatchingPair : IEquatable<MatchingPair>
    {
        public QuestionAnswer LeftAnswer;
        public QuestionAnswer RightAnswer;

        public MatchingPair(QuestionAnswer leftItem, QuestionAnswer rightItem)
        {
            LeftAnswer = leftItem;
            RightAnswer = rightItem;
        }

        public bool Equals(MatchingPair other)
        {
            return (LeftAnswer.ID == other.LeftAnswer.ID && RightAnswer.ID == other.RightAnswer.ID) ||
                   (LeftAnswer.ID == other.RightAnswer.ID && RightAnswer.ID == other.LeftAnswer.ID);
        }

        public override bool Equals(object obj) => obj is MatchingPair other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(LeftAnswer.ID, RightAnswer.ID);
    }

    public class MatchChoiceQuestion : QuestionBase
    {

        [SerializeField]
        [OnValueChanged(nameof(SyncAnswers))]
        private List<MatchingPair> _correctPairs = new();

        public List<QuestionAnswer> RightColumn => CorrectPairs.Select(p => p.RightAnswer).ToList();
        public List<QuestionAnswer> LeftColumn => CorrectPairs.Select(p => p.LeftAnswer).ToList();
        public List<MatchingPair> CorrectPairs => _correctPairs;

        private void OnValidate()
        {
            SyncAnswers();
        }

        public override bool CheckAnswers(QuestionAnswer[] answers)
        {
            if (answers == null || answers.Length == 0)
            {
                Logger.Log("No Answer");
                return false;
            }

            if (answers.Length != CorrectPairs.Count * 2)
            {
                Logger.Log("Didnt fully answer");
                return false;
            }

            foreach (QuestionAnswer answer in answers)
            {
                if (answer == null)
                {
                    Logger.Log("Answers is null");
                    return false;
                }
            }

            // Convert the flat array of answers into pairs for comparison
            var matchedPairs = new List<MatchingPair>();
            for (int i = 0; i < answers.Length; i += 2)
            {
                if (answers[i] == null || answers[i + 1] == null) return false;
                matchedPairs.Add(new MatchingPair(answers[i], answers[i + 1]));
            }

            foreach (var pair in matchedPairs)
            {
                if (!CorrectPairs.Any(cp => cp.Equals(pair)))
                {
                    return false;
                }
            }

            return true;
        }

        public void AddPair(QuestionAnswer left, QuestionAnswer right)
        {
            CorrectPairs.Add(new MatchingPair(left, right));
            SyncAnswers();
        }

        private void SyncAnswers()
        {
            if (CorrectPairs == null) return;

            List<QuestionAnswer> allAnswers = new List<QuestionAnswer>();
            foreach (var pair in CorrectPairs)
            {
                if (pair.LeftAnswer != null) allAnswers.Add(pair.LeftAnswer);
                if (pair.RightAnswer != null) allAnswers.Add(pair.RightAnswer);
            }

            Answers = allAnswers.ToArray();
        }
    }
}
