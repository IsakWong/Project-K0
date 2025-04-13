using System.Collections.Generic;
using DamageNumbersPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace K1.Gameplay
{
    public class HUDModule : KModule
    {
        public DamageNumber PhysicalDamageTextPrefab;
        public DamageNumber MagicDamageTextPrefab;
        public DamageNumber CounterTextPrefab;
        public DamageNumber EndureTextPrefab;
        public DamageNumber StunTextPrefab;
        public DamageNumber ParryTextPrefab;

        public GameObject Indicator;
        public GameObject IndicatorPrefab;
        public LineRenderer IndicatorLine;
        public Transform IndicatorCircle;

        public DamageNumber AbiTextPrefab;

        public GameObject mHealthbarPrefab;
        public GameObject mWarningCube;
        public GameObject mWarningCircle;

        public Material[] mWarningMats;

        public GameObject mTargetIndicator;
        public GameObject mMoveIndicator;

        public Canvas mHUDCanvas;

        // Start is called before the first frame update
        void Start()
        {
            Indicator = GameObject.Instantiate(IndicatorPrefab);
            IndicatorLine = Indicator.GetComponentInChildren<LineRenderer>();
            IndicatorCircle = Indicator.transform.GetChild(0);
            Indicator.SetActive(false);
            KGameCore.SystemAt<GameplayModule>().AnyEvent.OnTakeDamage += (hittedUnit, source, param) =>
            {
                var position = hittedUnit.GetSocketWorldPosition(BuiltinCharacterSocket.Body);
                bool isEnemy = KGameCore.SystemAt<PlayerModule>().GetLocalPlayerController<K1PlayerController>().mControlCharacter
                    .IsEnemy(hittedUnit);
                int value = (int)(param.DamageValue);
                switch (param.DamageType)
                {
                    case DamageType.MagicDamage:
                        MagicDamageTextPrefab.SpawnText(position, value.ToString());
                        break;
                    case DamageType.PhysicalDamage:
                        PhysicalDamageTextPrefab.SpawnText(position, value.ToString());
                        break;
                    case DamageType.TrueDamage:
                        PhysicalDamageTextPrefab.SpawnText(position, value.ToString());
                        break;
                }
            };
            KGameCore.SystemAt<GameplayModule>().AnyEvent.OnParry += (source, param) =>
            {
                var offset = Camera.main.transform.forward;
                offset.y = 0;
                if (source != param.PerriedUnit)
                    ParryTextPrefab.SpawnText(source.WorldPosition + offset * 1.5f, "格挡!");
            };
            KGameCore.SystemAt<GameplayModule>().AnyEvent.OnBreakAction += (counter, breakUnit) =>
            {
                var offset = Camera.main.transform.forward;
                offset.y = 0;
                if (counter != breakUnit)
                    CounterTextPrefab.SpawnText(breakUnit.WorldPosition + offset * 1.5f, "破招!");
            };
            KGameCore.SystemAt<GameplayModule>().AnyEvent.OnGetStuned += (unit, characterUnit, time) =>
            {
                StunTextPrefab.SpawnText(unit.WorldPosition, "击晕!");
            };
            KGameCore.SystemAt<GameplayModule>().AnyEvent.OnEndure += (source, unit) =>
            {
                var offset = Camera.main.transform.forward;
                offset.y = 0;
                if (source && unit != source)
                    EndureTextPrefab.SpawnText(unit.WorldPosition + offset * 1.5f, "免疫!");
            };
        }

        public void SpawnHud(DamageNumber damage, Vector3 position, string text)
        {
            var direction = Camera.main.transform.forward;
            direction.y = 0;
            direction.Normalize();
            Vector3 pos;
            if ((position - Camera.main.transform.position).magnitude < 4)
            {
                pos = position + direction * 2;
            }
            else
            {
                pos = position;
            }

            damage.SpawnText(pos, text);
        }

        public void OnAnyEndureOccur(CharacterUnit unit, CharacterUnit source)
        {
        }

        public void OnAnyCharBreakAction(CharacterUnit breakUnit, CharacterUnit counter)
        {
        }

        public void OnAnyCharParry(CharacterUnit source, ParryParam param)
        {
        }

        public void OnUnitGetHitted(CharacterUnit hittedUnit, CharacterUnit hitSource, DamageParam damage)
        {
        }

        private Dictionary<CharacterUnit, CharacterHUD> mHealthHuds = new();

        private void OnCharacterHealthChange(CharacterUnit target)
        {
            var characterHUD = mHealthHuds[target];
            var percent = target.HealthPercent;
            characterHUD.SetPercent(percent);
        }

        private void OnCharacterStatusChange(CharacterUnit target)
        {
            var characterHUD = mHealthHuds[target];
            var activeStateName = target.FSM.ActiveState;
            if (activeStateName is SoloActionCharacterState)
            {
                var actingState = activeStateName as SoloActionCharacterState;
                var percent = actingState.CastPercent;
                characterHUD.SetStatusLabel("释放");
                characterHUD.SetStatusVisible(true);
                characterHUD.SetStatusPercent(percent);
            }
            else if (activeStateName is StunCharacterState)
            {
                var stunState = activeStateName as StunCharacterState;
                var percent = stunState.RemainPercent;
                characterHUD.SetStatusVisible(true);
                characterHUD.SetStatusLabel("眩晕");
                characterHUD.SetStatusPercent(percent);
            }
            else if (activeStateName is HitCharacterState)
            {
                var stunState = activeStateName as HitCharacterState;
                var percent = stunState.RemainPercent;
                characterHUD.SetStatusVisible(true);
                characterHUD.SetStatusLabel("眩晕");
                characterHUD.SetStatusPercent(percent);
            }
            else
            {
                characterHUD.SetStatusVisible(false);
            }
        }

        public void UnRegisterHelathBarHUD(CharacterUnit target)
        {
            Destroy(mHealthHuds[target].gameObject);
            target.CharEvent.OnHealthChange -= OnCharacterHealthChange;
            target.CharEvent.OnStatusChange -= OnCharacterStatusChange;
            mHealthHuds.Remove(target);
        }

        public void RegisterCharacterHUD(CharacterUnit target, Vector3 offset)
        {
            if (mHealthHuds.ContainsKey(target))
                return;
            var healthbar = Instantiate(mHealthbarPrefab);
            var characterHUD = healthbar.GetComponent<CharacterHUD>();
            mHealthHuds.Add(target, characterHUD);
            // if (CharacterUnitAPI.IsEnemy(target, KGameCore.SystemAt<PlayerModule>().mPlayerController.mControlCharacter))
            // {
            //     characterHUD.SetColor(characterHUD.mEnemyColor);
            // }
            // else
            // {
            //     characterHUD.SetColor(characterHUD.mFriendColor);
            // }
            characterHUD.SetPercent(target.HealthPercent);
            //characterHUD.m2DCanvas = KGameCore.SystemAt<UIModule>().mHUDCanavs.transform;

            target.CharEvent.OnHealthChange += OnCharacterHealthChange;
            target.CharEvent.OnStatusChange += OnCharacterStatusChange;

            characterHUD.mTargetTransform = target.transform;
            characterHUD.mTargetOffset = offset;

            if (target.mIsHero)
            {
                characterHUD.mAvatarImg.gameObject.SetActive(true);
                characterHUD.SetAvatarImage(target.mCharacterConfig.Avatar);
            }
            else
            {
                characterHUD.mAvatarImg.gameObject.SetActive(false);
            }

            characterHUD.SetStatusVisible(false);
        }

        // Update is called once per frame
    }
}