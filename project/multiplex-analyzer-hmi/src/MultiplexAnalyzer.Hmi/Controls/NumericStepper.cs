using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MultiplexAnalyzer.Hmi.Controls;

[TemplatePart(Name = PartIncrease, Type = typeof(RepeatButton))]
[TemplatePart(Name = PartDecrease, Type = typeof(RepeatButton))]
public class NumericStepper : Control
{
    private const string PartIncrease = "PART_Increase";
    private const string PartDecrease = "PART_Decrease";

    private RepeatButton? increaseButton;
    private RepeatButton? decreaseButton;

    static NumericStepper()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumericStepper),
            new FrameworkPropertyMetadata(typeof(NumericStepper)));
    }

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(NumericStepper),
        new FrameworkPropertyMetadata(
            0d,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnValueChanged,
            CoerceValue));

    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
        nameof(Minimum),
        typeof(double),
        typeof(NumericStepper),
        new FrameworkPropertyMetadata(0d, OnRangeChanged));

    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum),
        typeof(double),
        typeof(NumericStepper),
        new FrameworkPropertyMetadata(100d, OnRangeChanged));

    public static readonly DependencyProperty StepProperty = DependencyProperty.Register(
        nameof(Step),
        typeof(double),
        typeof(NumericStepper),
        new PropertyMetadata(1d));

    public static readonly DependencyProperty UnitProperty = DependencyProperty.Register(
        nameof(Unit),
        typeof(string),
        typeof(NumericStepper),
        new PropertyMetadata(string.Empty));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }


    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (decreaseButton is not null)
        {
            decreaseButton.Click -= OnDecreaseClick;
        }

        if (increaseButton is not null)
        {
            increaseButton.Click -= OnIncreaseClick;
        }

        decreaseButton = GetTemplateChild(PartDecrease) as RepeatButton;
        increaseButton = GetTemplateChild(PartIncrease) as RepeatButton;

        if (decreaseButton is not null)
        {
            decreaseButton.Click += OnDecreaseClick;
        }

        if (increaseButton is not null)
        {
            increaseButton.Click += OnIncreaseClick;
        }

        UpdateButtonAvailability();
    }

    private void OnDecreaseClick(object sender, RoutedEventArgs e) => Value -= Step;

    private void OnIncreaseClick(object sender, RoutedEventArgs e) => Value += Step;

    private static object CoerceValue(DependencyObject d, object baseValue)
    {
        var stepper = (NumericStepper)d;
        var clamped = Math.Clamp((double)baseValue, stepper.Minimum, stepper.Maximum);
        return Math.Round(clamped, 4);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((NumericStepper)d).UpdateButtonAvailability();

    private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.CoerceValue(ValueProperty);
        ((NumericStepper)d).UpdateButtonAvailability();
    }

    private void UpdateButtonAvailability()
    {
        if (decreaseButton is not null)
        {
            decreaseButton.IsEnabled = Value > Minimum;
        }

        if (increaseButton is not null)
        {
            increaseButton.IsEnabled = Value < Maximum;
        }
    }
}
