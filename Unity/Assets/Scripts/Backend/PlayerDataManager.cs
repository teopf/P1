using System;
using UnityEngine;

namespace Backend
{
    public class PlayerDataManager : MonoBehaviour
    {
        public static PlayerDataManager Instance { get; private set; }

        public GameData CurrentData { get; private set; }

        // 데이터 변경 시 발생하는이벤트 (변경된 데이터를 인자로 전달)
        public event Action<GameData> OnDataChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                CurrentData = new GameData(); // 초기 빈 데이터
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // 초기 데이터 로드 등을 위한 더미 메서드 (추후 실제 서버 연동 필요)
        public void InitializeData()
        {
            // TODO: 서버에서 실제 데이터를 로드하는 로직으로 대체 예정
            RefreshUI();
        }

        // 서버로부터 받은 최신 데이터로 로컬 캐시를 갱신
        public void UpdateData(GameData newData)
        {
            if (newData == null) return;

            CurrentData = newData;
            NotifyDataChanged();
        }

        // 이벤트를 발생시켜 구독 중인 UI들을 갱신
        private void NotifyDataChanged()
        {
            OnDataChanged?.Invoke(CurrentData);
            Debug.Log($"PlayerDataManager: Data updated. Gold: {CurrentData.Gold}, Gem: {CurrentData.Gem}");
        }

        // 강제로 UI 갱신이 필요할 때 호출
        public void RefreshUI()
        {
            NotifyDataChanged();
        }
    }
}
