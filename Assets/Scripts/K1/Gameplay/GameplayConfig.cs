using Framework.Config;
using UnityEngine;
using UnityEngine.Serialization;

namespace K1.Gameplay
{
    [CreateAssetMenu(fileName = "GameplayConfig", menuName = "GameplayConfig")]
    public class GameplayConfig : ConfigObject
    {
        public static GameplayConfig Instance()
        {
            return KGameCore.SystemAt<ConfigModule>().GetConfig<GameplayConfig>();
        }

        public Vfx DefaultBox;
        public Vfx DefaultSpere;
        public GameObject BloodEffect;


        public BuffConfig DefaultEndureBuff;
        public BuffConfig DefaultInvicible;
        public BuffConfig DefaultStun;

        public StunBuff CreateStunBuff()
        {
            return DefaultStun.CreateBuff() as StunBuff;
        }

        public BuffConfig DefaultPropertyModify;
        public BuffConfig DefaultMovement;
        public BuffConfig DefaultRepellMovement;
    }
}