using System;

namespace Backend
{
    [Serializable]
    public class GameData
    {
        public int Gold;
        public int Gem;
        public int Level;
        public int Exp;

        // 생성자에서 초기값 설정
        public GameData()
        {
            Gold = 0;
            Gem = 0;
            Level = 1;
            Exp = 0;
        }
    }
}
