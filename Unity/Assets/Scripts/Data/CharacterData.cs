using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// 영웅 등급
    /// </summary>
    public enum HeroGrade
    {
        Rare,       // 레어
        Epic,       // 에픽
        Relic,      // 렐릭
        Mystic      // 미스틱
    }

    /// <summary>
    /// 캐릭터 기본 데이터 (성급 참조)
    /// </summary>
    [CreateAssetMenu(fileName = "New Character", menuName = "Game Data/Character")]
    public class CharacterData : ScriptableObject
    {
        [Header("기본 정보")]
        [Tooltip("캐릭터 표기 이름")]
        public string characterName;
        
        [Tooltip("영웅 등급")]
        public HeroGrade heroGrade = HeroGrade.Rare;

        [Header("성급별 스펙")]
        [Tooltip("1성 스펙")]
        public CharacterSpecData star1Spec;
        
        [Tooltip("2성 스펙")]
        public CharacterSpecData star2Spec;
        
        [Tooltip("3성 스펙")]
        public CharacterSpecData star3Spec;
        
        [Tooltip("4성 스펙")]
        public CharacterSpecData star4Spec;
        
        [Tooltip("5성 스펙")]
        public CharacterSpecData star5Spec;

        /// <summary>
        /// 성급에 따른 스펙 가져오기
        /// </summary>
        public CharacterSpecData GetSpecByStarCount(int starCount)
        {
            return starCount switch
            {
                1 => star1Spec,
                2 => star2Spec,
                3 => star3Spec,
                4 => star4Spec,
                5 => star5Spec,
                _ => star1Spec
            };
        }
    }
}
