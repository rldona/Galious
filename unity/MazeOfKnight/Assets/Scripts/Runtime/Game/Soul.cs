namespace MazeOfKnight.Game
{
    // RemakeRules.HollowSoul: el alma de Hollow Knight en lugar de la experiencia del original (lo pidió el usuario).
    // Cada espadazo que acierta da HitGain (el doble con el Vase, que en el original doblaba la experiencia); se guarda
    // hasta Max. Concentrarse (mantener el botón, quieto en el suelo) dura FocusTicks y gasta FocusCost para curar
    // FocusHeal unidades de vitalidad (máscaras); si se sigue manteniendo, vuelve a empezar.
    public class Soul
    {
        public const int Max = 99, HitGain = 11, FocusCost = 33, FocusHeal = 2, FocusTicks = 27;   // 0,9 s, como Hollow Knight

        public int Value;
        public int FocusTick { get; private set; }     // ticks concentrándose (0 = no)
        public bool Focusing => FocusTick > 0;
        public float FocusProgress => FocusTick / (float)FocusTicks;   // 0 → 1 hasta curar

        public void Gain(bool vase) => Value = System.Math.Min(Max, Value + (vase ? HitGain * 2 : HitGain));

        public bool CanFocus => Value >= FocusCost;

        // Un tick. holding: el botón mantenido; able: quieto en el suelo, sin golpear ni recibir golpes y sin la vida
        // llena. Devuelve las unidades curadas este tick (0 casi siempre).
        public int Tick(bool holding, bool able)
        {
            if (!holding || !able || !CanFocus && FocusTick == 0)
            {
                FocusTick = 0;
                return 0;
            }
            if (++FocusTick < FocusTicks) return 0;
            FocusTick = 0;
            Value -= FocusCost;
            return FocusHeal;
        }

        public void Cancel() => FocusTick = 0;
    }
}
