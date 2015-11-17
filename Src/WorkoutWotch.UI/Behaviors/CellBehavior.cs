namespace WorkoutWotch.UI.Behaviors
{
    using Xamarin.Forms;

    public static class CellBehavior
    {
        public static BindableProperty AccessoryProperty = BindableProperty.CreateAttached(
            "Accessory",
            typeof(AccessoryType),
            typeof(Cell),
            AccessoryType.None);

        public static BindableProperty IsSelectableProperty = BindableProperty.CreateAttached(
            "IsSelectable",
            typeof(bool),
            typeof(Cell),
            true);

        public static AccessoryType GetAccessory(BindableObject bindableObject) =>
            (AccessoryType)bindableObject.GetValue(AccessoryProperty);

        public static void SetAccessory(BindableObject bindableObject, AccessoryType accessoryType) =>
            bindableObject.SetValue(AccessoryProperty, accessoryType);

        public static bool GetIsSelectable(BindableObject bindableObject) =>
            (bool)bindableObject.GetValue(IsSelectableProperty);

        public static void SetIsSelectable(BindableObject bindableObject, bool isSelectable) =>
            bindableObject.SetValue(IsSelectableProperty, isSelectable);
    }
}