namespace K1.Gameplay
{
    //属性修改
    public class PropertyModifyBuff : Buffbase
    {
        public VariantRef<bool> IsFixed = new(true);
        public VariantRef<float> Increment = new(10.0f);

        public CharacterUnit.CharacterProperty mModifierType = CharacterUnit.CharacterProperty.Health;

        private PropertyModifier _Modifier;

        public override void BuffAdd()
        {
            base.BuffAdd();
            _Modifier = BuffOwner.AddPropertyModifier(mModifierType, IsFixed, Increment);
        }

        public override void BuffEnd()
        {
            base.BuffEnd();
            BuffOwner.RemovePropertyModifier(_Modifier);
        }
    }
}