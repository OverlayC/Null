namespace NullEx
{
    public sealed class EspFeature : Feature
    {
        public override string Name => "ESP";
        public override string Description => "Draw boxes, names, health, distance";
        protected override int UpdateIntervalMs => 250;

        public EspFeature()
        {
            ToggleKey = 0;
            Set("ShowBox", true);
            Set("ShowName", true);
            Set("ShowHealth", false);
            Set("ShowDistance", false);
            Set("BoxThickness", 2);
            Set("WidthRatio", 50);
            Set("ColorR", 0);
            Set("ColorG", 255);
            Set("ColorB", 0);
            Set("MaxDistance", 0);

            PersistentKeys.Add("ShowBox");
            PersistentKeys.Add("ShowName");
            PersistentKeys.Add("ShowHealth");
            PersistentKeys.Add("ShowDistance");
            PersistentKeys.Add("BoxThickness");
            PersistentKeys.Add("WidthRatio");
            PersistentKeys.Add("ColorR");
            PersistentKeys.Add("ColorG");
            PersistentKeys.Add("ColorB");
            PersistentKeys.Add("MaxDistance");
        }

        public bool ShowBox { get => Get<bool>("ShowBox"); set => Set("ShowBox", value); }
        public bool ShowName { get => Get<bool>("ShowName"); set => Set("ShowName", value); }
        public bool ShowHealth { get => Get<bool>("ShowHealth"); set => Set("ShowHealth", value); }
        public bool ShowDistance { get => Get<bool>("ShowDistance"); set => Set("ShowDistance", value); }
        public int BoxThickness { get => Get<int>("BoxThickness"); set => Set("BoxThickness", value); }
        public int WidthRatio { get => Get<int>("WidthRatio"); set => Set("WidthRatio", value); }
        public int MaxDistance { get => Get<int>("MaxDistance"); set => Set("MaxDistance", value); }
        public int ColorR { get => Get<int>("ColorR"); set => Set("ColorR", value); }
        public int ColorG { get => Get<int>("ColorG"); set => Set("ColorG", value); }
        public int ColorB { get => Get<int>("ColorB"); set => Set("ColorB", value); }

        protected override void OnEnable() => Overlay.Start();
        protected override void OnDisable() => Overlay.Stop();
        protected override void OnUpdate() { }
    }
}