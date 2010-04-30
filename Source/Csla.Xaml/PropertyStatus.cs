﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Csla.Core;
using Csla.Reflection;
using Csla.Rules;

namespace Csla.Xaml
{
  /// <summary>
  /// Displays validation information for a business
  /// object property, and manipulates an associated
  /// UI control based on the business object's
  /// authorization rules.
  /// </summary>
  [TemplatePart(Name = "image", Type = typeof(FrameworkElement))]
  [TemplatePart(Name = "popup", Type = typeof(Popup))]
  [TemplatePart(Name = "busy", Type = typeof(BusyAnimation))]
  [TemplateVisualState(Name = "Valid", GroupName = "CommonStates")]
  [TemplateVisualState(Name = "Error", GroupName = "CommonStates")]
  [TemplateVisualState(Name = "Warning", GroupName = "CommonStates")]
  [TemplateVisualState(Name = "Information", GroupName = "CommonStates")]
  public class PropertyStatus : ContentControl,
    INotifyPropertyChanged
  {
    private bool _isReadOnly = false; 
    private FrameworkElement _lastImage;
    private Point _lastPosition;
    private Point _popupLastPosition;
    private Size _lastAppSize;
    private Size _lastPopupSize;
    
    /// <summary>
    /// Gets or sets a value indicating whether this DependencyProperty field is read only.
    /// </summary>
    /// <value>
    /// <c>true</c> if this DependencyProperty is read only; otherwise, <c>false</c>.
    /// </value>
    protected bool IsReadOnly
    {
      get
      {
        return _isReadOnly;
      }
      set
      {
        _isReadOnly = value;
      }
    }


    #region Constructors

    /// <summary>
    /// Creates an instance of the object.
    /// </summary>
    public PropertyStatus()
      : base()
    {
      BrokenRules = new ObservableCollection<BrokenRule>();
      DefaultStyleKey = typeof(PropertyStatus);
      IsTabStop = false;

      Loaded += (o, e) => GoToState(true);
    }

    /// <summary>
    /// Applies the visual template.
    /// </summary>
    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      UpdateState();
    }

    #endregion

    #region Source property

    /// <summary>
    /// Gets or sets the source business
    /// property to which this control is bound.
    /// </summary>
    public static readonly DependencyProperty PropertyProperty = DependencyProperty.Register(
      "Property",
      typeof(object),
      typeof(PropertyStatus),
      new PropertyMetadata(new object(), (o, e) => ((PropertyStatus)o).SetSource()));

    /// <summary>
    /// Gets or sets the source business
    /// property to which this control is bound.
    /// </summary>
    [Category("Common")]
    public object Property
    {
      get { return GetValue(PropertyProperty); }
      set
      {
        SetValue(PropertyProperty, value);
        SetSource();
      }
    }

    private object _source = null;
    /// <summary>
    /// Gets or sets the Source.
    /// </summary>
    /// <value>The source.</value>
    protected object Source
    {
      get
      {
        return _source;
      }
      set
      {
        _source = value;
      }
    }

    private string _bindingPath = string.Empty;
    /// <summary>
    /// Gets or sets the binding path.
    /// </summary>
    /// <value>The binding path.</value>
    protected string BindingPath
    {
      get
      {
        return _bindingPath;
      }
      set
      {
        _bindingPath = value;
      }
    }

    /// <summary>
    /// Sets the source binding and updates status.
    /// </summary>
    protected virtual void SetSource()
    {
      var old = Source;
      var binding = GetBindingExpression(PropertyProperty);
      if (binding != null)
      {
        if (binding.ParentBinding != null && binding.ParentBinding.Path != null)
          BindingPath = binding.ParentBinding.Path.Path;
        else
          BindingPath = string.Empty;
        Source = GetRealSource(binding.DataItem, BindingPath);
        if (BindingPath.IndexOf('.') > 0)
          BindingPath = BindingPath.Substring(BindingPath.LastIndexOf('.') + 1);
      }
      else
      {
        Source = null;
        BindingPath = string.Empty;
      }

      HandleIsBusy(old, Source);

      FindIsReadOnly();
      UpdateState();
    }

    /// <summary>
    /// Gets the real source helper method.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="bindingPath">The binding path.</param>
    /// <returns></returns>
    protected object GetRealSource(object source, string bindingPath)
    {
      var icv = source as ICollectionView;
      if (icv != null)
        source = icv.CurrentItem;
      if (source != null && bindingPath.IndexOf('.') > 0)
      {
        var firstProperty = bindingPath.Substring(0, bindingPath.IndexOf('.'));
        var p = MethodCaller.GetProperty(source.GetType(), firstProperty);
        return GetRealSource(
          MethodCaller.GetPropertyValue(source, p),
          bindingPath.Substring(bindingPath.IndexOf('.') + 1));
      }
      else
        return source;
    }

    private void HandleIsBusy(object old, object source)
    {
      if (!ReferenceEquals(old, source))
      {
        DetachSource(old);
        AttachSource(source);
        BusinessBase bb = Source as BusinessBase;
        if (bb != null)
        {
          IsBusy = bb.IsPropertyBusy(BindingPath);
        }
      }
    }

    private void DetachSource(object source)
    {
      var p = source as INotifyPropertyChanged;
      if (p != null)
        p.PropertyChanged -= source_PropertyChanged;
      INotifyBusy busy = source as INotifyBusy;
      if (busy != null)
        busy.BusyChanged -= source_BusyChanged;
    }

    private void AttachSource(object source)
    {
      var p = source as INotifyPropertyChanged;
      if (p != null)
        p.PropertyChanged += source_PropertyChanged;
      INotifyBusy busy = source as INotifyBusy;
      if (busy != null)
        busy.BusyChanged += source_BusyChanged;
    }

    void source_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == BindingPath)
        UpdateState();
    }

    void source_BusyChanged(object sender, BusyChangedEventArgs e)
    {
      if (e.PropertyName == BindingPath)
      {
        bool busy = e.Busy;
        BusinessBase bb = Source as BusinessBase;
        if (bb != null)
          busy = bb.IsPropertyBusy(BindingPath);

        if (busy != IsBusy)
        {
          IsBusy = busy;
          UpdateState();
        }
      }
    }

    #endregion

    #region Target property

    /// <summary>
    /// Gets or sets the target control to which this control is bound.
    /// THIS FEATURE IS OBSOLETE AND SHOULD NOT BE USED.
    /// </summary>
    public static readonly DependencyProperty TargetControlProperty = DependencyProperty.Register(
      "TargetControl",
      typeof(object),
      typeof(PropertyStatus),
      new PropertyMetadata(null, (o, e) => { ((PropertyStatus)o).HandleTarget(); }));

    /// <summary>
    /// Gets or sets the target control to which this control is bound.
    /// THIS FEATURE IS OBSOLETE AND SHOULD NOT BE USED.
    /// </summary>
    public object TargetControl
    {
      get { return GetValue(TargetControlProperty); }
      set { SetValue(TargetControlProperty, value); }
    }

    #endregion

    #region BrokenRules property

    /// <summary>
    /// Gets the broken rules collection from the
    /// business object.
    /// </summary>
    public static readonly DependencyProperty BrokenRulesProperty = DependencyProperty.Register(
      "BrokenRules",
      typeof(ObservableCollection<BrokenRule>),
      typeof(PropertyStatus),
      null);

    /// <summary>
    /// Gets the broken rules collection from the
    /// business object.
    /// </summary>
    [Category("Property Status")]
    public ObservableCollection<BrokenRule> BrokenRules
    {
      get { return (ObservableCollection<BrokenRule>)GetValue(BrokenRulesProperty); }
      private set { SetValue(BrokenRulesProperty, value); }
    }

    #endregion

    #region State properties

    private bool _canRead = true;
    /// <summary>
    /// Gets a value indicating whether the user
    /// is authorized to read the property.
    /// </summary>
    [Category("Property Status")]
    public bool CanRead
    {
      get { return _canRead; }
      protected set
      {
        if (value != _canRead)
        {
          _canRead = value;
          OnPropertyChanged("CanRead");
        }
      }
    }

    private bool _canWrite = true;
    /// <summary>
    /// Gets a value indicating whether the user
    /// is authorized to write the property.
    /// </summary>
    [Category("Property Status")]
    public bool CanWrite
    {
      get { return _canWrite; }
      protected set
      {
        if (value != _canWrite)
        {
          _canWrite = value;
          OnPropertyChanged("CanWrite");
        }
      }
    }

    private bool _isBusy = false;
    /// <summary>
    /// Gets a value indicating whether the property
    /// is busy with an asynchronous operation.
    /// </summary>
    [Category("Property Status")]
    public bool IsBusy
    {
      get { return _isBusy; }
      private set
      {
        if (value != _isBusy)
        {
          _isBusy = value;
          OnPropertyChanged("IsBusy");
        }
      }
    }

    private bool _isValid = true;
    /// <summary>
    /// Gets a value indicating whether the 
    /// property is valid.
    /// </summary>
    [Category("Property Status")]
    public bool IsValid
    {
      get { return _isValid; }
      private set
      {
        if (value != _isValid)
        {
          _isValid = value;
          OnPropertyChanged("IsValid");
        }
      }
    }

    private RuleSeverity _worst;
    /// <summary>
    /// Gets a valud indicating the worst
    /// severity of all broken rules
    /// for this property (if IsValid is
    /// false).
    /// </summary>
    [Category("Property Status")]
    public RuleSeverity RuleSeverity
    {
      get { return _worst; }
      private set
      {
        if (value != _worst)
        {
          _worst = value;
          OnPropertyChanged("RuleSeverity");
        }
      }
    }

    private string _ruleDescription = string.Empty;
    /// <summary>
    /// Gets the description of the most severe
    /// broken rule for this property.
    /// </summary>
    [Category("Property Status")]
    public string RuleDescription
    {
      get { return _ruleDescription; }
      private set
      {
        if (value != _ruleDescription)
        {
          _ruleDescription = value;
          OnPropertyChanged("RuleDescription");
        }
      }
    }

    #endregion

    #region Image

    private void EnablePopup(FrameworkElement image)
    {
      if (image != null)
      {
        image.MouseEnter += new MouseEventHandler(image_MouseEnter);
        image.MouseLeave += new MouseEventHandler(image_MouseLeave);
      }
    }

    private void DisablePopup(FrameworkElement image)
    {
      if (image != null)
      {
        image.MouseEnter -= new MouseEventHandler(image_MouseEnter);
        image.MouseLeave -= new MouseEventHandler(image_MouseLeave);
      }
    }

    private void image_MouseEnter(object sender, MouseEventArgs e)
    {
      Popup popup = (Popup)FindChild(this, "popup");
#if SILVERLIGHT
      if (popup != null && sender is UIElement && Application.Current.RootVisual != null)
      {
        _lastPosition = e.GetPosition(Application.Current.RootVisual);
        _lastAppSize = Application.Current.RootVisual.RenderSize;
        Point p = e.GetPosition((UIElement)sender);
        Size size = ((UIElement)sender).DesiredSize;
        // ensure events are attached only once.
        popup.Child.MouseLeave -= new MouseEventHandler(popup_MouseLeave);
        popup.Child.MouseLeave += new MouseEventHandler(popup_MouseLeave);
        ((ItemsControl)popup.Child).ItemsSource = BrokenRules;

        popup.VerticalOffset = p.Y + size.Height;
        popup.HorizontalOffset = p.X + size.Width;
        _popupLastPosition = new Point();
        _popupLastPosition.X = popup.HorizontalOffset;
        _popupLastPosition.Y = popup.VerticalOffset;
        popup.Loaded += popup_Loaded;
        popup.IsOpen = true;
      }
#else
      if(popup != null && sender is UIElement)
      {
        popup.Placement = PlacementMode.Mouse;
        popup.PlacementTarget = (UIElement)sender;
        ((ItemsControl)popup.Child).ItemsSource = BrokenRules;
        popup.IsOpen = true;
      }
#endif
    }
    
    void popup_Loaded(object sender, RoutedEventArgs e)
    {
      (sender as Popup).Loaded -= popup_Loaded;
      if (((sender as Popup).Child as UIElement).DesiredSize.Height > 0)
      {
        _lastPopupSize = ((sender as Popup).Child as UIElement).DesiredSize;
      }
      if (_lastAppSize != null && _lastPosition != null && _popupLastPosition != null && _lastPopupSize != null)
      {
        if (_lastAppSize.Width < _lastPosition.X + _popupLastPosition.X + _lastPopupSize.Width)
        {
          (sender as Popup).HorizontalOffset = _lastAppSize.Width - _lastPosition.X - _popupLastPosition.X - _lastPopupSize.Width;
        }
        if (_lastAppSize.Height < _lastPosition.Y + _popupLastPosition.Y + _lastPopupSize.Height)
        {
          (sender as Popup).VerticalOffset = _lastAppSize.Height - _lastPosition.Y - _popupLastPosition.Y - _lastPopupSize.Height;
        }
      }
    }

    private void image_MouseLeave(object sender, MouseEventArgs e)
    {
      Popup popup = (Popup)FindChild(this, "popup");
      popup.IsOpen = false;
    }

    void popup_MouseLeave(object sender, MouseEventArgs e)
    {
      Popup popup = (Popup)FindChild(this, "popup");
      popup.IsOpen = false;
    }

    #endregion

    #region State management

    /// <summary>
    /// Updates the state on control Property.
    /// </summary>
    protected virtual void UpdateState()
    {
      Popup popup = (Popup)FindChild(this, "popup");
      if (popup != null)
        popup.IsOpen = false;

      BusinessBase businessObject = Source as BusinessBase;
      if (businessObject != null)
      {
        var allRules = (from r in businessObject.BrokenRulesCollection
                        where r.Property == BindingPath
                        select r).ToArray();

        var removeRules = (from r in BrokenRules
                           where !allRules.Contains(r)
                           select r).ToArray();

        var addRules = (from r in allRules
                        where !BrokenRules.Contains(r)
                        select r).ToArray();

        foreach (var rule in removeRules)
          BrokenRules.Remove(rule);
        foreach (var rule in addRules)
          BrokenRules.Add(rule);

        BrokenRule worst = (from r in BrokenRules
                            orderby r.Severity
                            select r).FirstOrDefault();

        if (worst != null)
        {
          RuleSeverity = worst.Severity;
          RuleDescription = worst.Description;
        }
        else
          RuleDescription = string.Empty;

        IsValid = BrokenRules.Count == 0;
        GoToState(true);
      }
      else
      {
        BrokenRules.Clear();
        RuleDescription = string.Empty;
        IsValid = true;
        GoToState(true);
      }
    }

    /// <summary>
    /// Updates the status of the Property in UI
    /// </summary>
    /// <param name="useTransitions">if set to <c>true</c> then use transitions.</param>
    protected virtual void GoToState(bool useTransitions)
    {
      DisablePopup(_lastImage);
      HandleTarget();

      BusyAnimation busy = FindChild(this, "busy") as BusyAnimation;
      if (busy != null)
        busy.IsRunning = IsBusy;

      if (IsBusy)
      {
        VisualStateManager.GoToState(this, "Busy", useTransitions);
      }
      else if (IsValid)
      {
        VisualStateManager.GoToState(this, "Valid", useTransitions);
      }
      else
      {
        VisualStateManager.GoToState(this, RuleSeverity.ToString(), useTransitions);
        _lastImage = (FrameworkElement)FindChild(this, string.Format("{0}Image", RuleSeverity.ToString().ToLower()));
        EnablePopup(_lastImage);
      }
    }

    #endregion

    #region TargetControl

    /// <summary>
    /// Update the ReadOnly status of DependencyProperty 
    /// </summary>
    protected virtual void FindIsReadOnly()
    {
      if (Source != null && !string.IsNullOrEmpty(BindingPath))
      {
        var info = Source.GetType().GetProperty(BindingPath);
        if (info != null)
        {
          IsReadOnly = !info.CanWrite;
          if (!IsReadOnly)
          {
            var setter = Source.GetType().GetMethod("set_" + BindingPath);
            if (setter == null)
              IsReadOnly = true;
            else
              IsReadOnly = !setter.IsPublic;
          }
        }
      }
      else
      {
        IsReadOnly = false;
      }
    }


    /// <summary>
    /// Set the status on the target Property. 
    /// 
    /// Set IsReadOnly if control implements this property else set IsEnabled. 
    /// If user is alloed to read the DependencyProperty then hide the actual value 
    /// by pverriding the value to null or string.Empty.
    /// </summary>
    protected virtual void HandleTarget()
    {
      if (!string.IsNullOrEmpty(BindingPath))
      {
        var b = Source as Csla.Security.IAuthorizeReadWrite;
        if (b != null)
        {
          CanWrite = b.CanWriteProperty(BindingPath);
          if (TargetControl != null)
          {
            if (CanWrite && !IsReadOnly)
            {
              if (MethodCaller.IsMethodImplemented(TargetControl, "set_IsReadOnly", false))
                MethodCaller.CallMethod(TargetControl, "set_IsReadOnly", false);
              else
                MethodCaller.CallMethodIfImplemented(TargetControl, "set_IsEnabled", true);
            }
            else
            {
              if (MethodCaller.IsMethodImplemented(TargetControl, "set_IsReadOnly", true))
                MethodCaller.CallMethod(TargetControl, "set_IsReadOnly", true);
              else
                MethodCaller.CallMethodIfImplemented(TargetControl, "set_IsEnabled", false);
            }
          }

          CanRead = b.CanReadProperty(BindingPath);
          if (TargetControl != null && !CanRead)
          {
            if (MethodCaller.IsMethodImplemented(TargetControl, "set_Content", null))
              MethodCaller.CallMethod(TargetControl, "set_Content", null);
            else
              MethodCaller.CallMethodIfImplemented(TargetControl, "set_Text", "");
          }
        }
      }
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Find child dependency property.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="name">The name.</param>
    /// <returns>DependencyObject child</returns>
    protected DependencyObject FindChild(DependencyObject parent, string name)
    {
      DependencyObject found = null;
      int count = VisualTreeHelper.GetChildrenCount(parent);
      for (int x = 0; x < count; x++)
      {
        DependencyObject child = VisualTreeHelper.GetChild(parent, x);
        string childName = child.GetValue(FrameworkElement.NameProperty) as string;
        if (childName == name)
        {
          found = child;
          break;
        }
        else found = FindChild(child, name);
      }

      return found;
    }

    #endregion

    #region INotifyPropertyChanged Members

    /// <summary>
    /// Event raised when a property has changed.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">Name of the changed property.</param>
    protected virtual void OnPropertyChanged(string propertyName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
  }
}
