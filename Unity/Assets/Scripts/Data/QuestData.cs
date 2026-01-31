using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// 퀘스트 타입 정의
    /// </summary>
    public enum QuestType
    {
        Daily,       // 일일 퀘스트
        Weekly,      // 주간 퀘스트
        Achievement, // 업적 퀘스트
        Repeat       // 반복 퀘스트
    }

    /// <summary>
    /// 퀘스트 상태 정의
    /// </summary>
    public enum QuestStatus
    {
        Locked,      // 잠김 (조건 미충족)
        InProgress,  // 진행중
        Completed,   // 완료 (보상 수령 가능)
        Claimed      // 보상 수령 완료
    }

    /// <summary>
    /// 퀘스트 데이터 클래스
    /// </summary>
    [Serializable]
    public class QuestData
    {
        public int id;                      // 퀘스트 고유 ID
        public string title;                // 퀘스트 제목
        public string description;          // 퀘스트 설명
        public QuestType type;              // 퀘스트 타입
        public QuestStatus status;          // 퀘스트 상태
        public int currentProgress;         // 현재 진행도
        public int goalProgress;            // 목표 진행도
        public int iconId;                  // 아이콘 ID
        public int goldReward;              // 골드 보상
        public int gemReward;               // 젬 보상
        public int expReward;               // 경험치 보상

        /// <summary>
        /// 진행률 계산 (0.0 ~ 1.0)
        /// </summary>
        public float GetProgressRatio()
        {
            if (goalProgress <= 0) return 0f;
            return Mathf.Clamp01((float)currentProgress / goalProgress);
        }

        /// <summary>
        /// 퀘스트 완료 여부 확인
        /// </summary>
        public bool IsCompleted()
        {
            return currentProgress >= goalProgress && status == QuestStatus.Completed;
        }
    }

    /// <summary>
    /// Mock 퀘스트 데이터 생성 유틸리티
    /// </summary>
    public static class QuestDataMock
    {
        /// <summary>
        /// 카테고리별 Mock 퀘스트 데이터 생성
        /// </summary>
        /// <param name="type">퀘스트 타입</param>
        /// <returns>Mock 퀘스트 리스트</returns>
        public static List<QuestData> GetMockQuests(QuestType type)
        {
            List<QuestData> quests = new List<QuestData>();
            int count = 0;

            switch (type)
            {
                case QuestType.Daily:
                    count = 10;
                    break;
                case QuestType.Weekly:
                    count = 5;
                    break;
                case QuestType.Achievement:
                    count = 15;
                    break;
                case QuestType.Repeat:
                    count = 8;
                    break;
            }

            for (int i = 0; i < count; i++)
            {
                quests.Add(new QuestData
                {
                    id = (int)type * 1000 + i,
                    title = $"{type} Quest {i + 1}",
                    description = $"Complete {type} task {i + 1}",
                    type = type,
                    status = GetRandomStatus(i),
                    currentProgress = UnityEngine.Random.Range(0, 20),
                    goalProgress = UnityEngine.Random.Range(5, 20),
                    iconId = i % 10,
                    goldReward = 100 * (i + 1),
                    gemReward = 10 * (i + 1),
                    expReward = 50 * (i + 1)
                });
            }

            return quests;
        }

        /// <summary>
        /// 랜덤 퀘스트 상태 생성 (테스트용)
        /// </summary>
        private static QuestStatus GetRandomStatus(int index)
        {
            int rand = index % 4;
            switch (rand)
            {
                case 0: return QuestStatus.Locked;
                case 1: return QuestStatus.InProgress;
                case 2: return QuestStatus.Completed;
                default: return QuestStatus.Claimed;
            }
        }
    }
}
