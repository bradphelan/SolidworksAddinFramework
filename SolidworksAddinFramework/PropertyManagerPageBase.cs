using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;

namespace SolidworksAddinFramework
{
    /// <summary>
    /// Base class for all property manager pages. See sample for more info
    /// </summary>
    /// <typeparam name="TMacroFeature">The type of the macro feature this page is designed for</typeparam>
    /// <typeparam name="TData">The type of the macro feature database this page will serialize data to and from</typeparam>
    [ComVisible(false)]
    public abstract class PropertyManagerPageBase<TMacroFeature,TData> : IPropertyManagerPage2Handler9
        where TData : MacroFeatureDataBase, new()
        where TMacroFeature : MacroFeatureBase<TMacroFeature,TData>
    {
        public readonly ISldWorks SwApp;
        private readonly string _Name;
        private readonly IEnumerable<swPropertyManagerPageOptions_e> _OptionsE;

        public TMacroFeature MacroFeature { get; set; }

        protected PropertyManagerPageBase(string name, IEnumerable<swPropertyManagerPageOptions_e> optionsE, TMacroFeature macroFeature)
        {
            if (macroFeature == null) throw new ArgumentNullException(nameof(macroFeature));

            SwApp = macroFeature.SwApp;
            _Name = name;
            _OptionsE = optionsE;
            MacroFeature = macroFeature;
        }

        private bool _ControlsAdded = false;

        public void Show()
        {
            var options = _OptionsE.Aggregate(0,(acc,v)=>(int)v | acc);
            var errors = 0;
            var propertyManagerPage = SwApp.CreatePropertyManagerPage(_Name, options, new PropertyManagerPage2Handler9Wrapper(this), ref errors);
            Page = (IPropertyManagerPage2)propertyManagerPage;
            if (Page != null && errors == (int) swPropertyManagerPageStatus_e.swPropertyManagerPage_Okay)
            {
            }
            else
            {
                throw new Exception("Unable to Create PMP");
            }
            if(!_ControlsAdded)
                AddControls();
            _ControlsAdded = true;
            Page?.Show();
        }

        private void AddControls()
        {
            _Disposables = AddControlsImpl().ToList();
        }

        /// <summary>
        /// Implement this method to add all controls to the page. See sample for more info 
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<IDisposable> AddControlsImpl();


        /// <summary>
        /// The instance of the real solid works property manager page. You will still have to call some
        /// methods on this. Not all magic is done automatically.
        /// </summary>
        public IPropertyManagerPage2 Page { get; set; }

        public virtual void AfterActivation()
        {
        }

        public void OnClose(int reason)
        {
            OnClose((swPropertyManagerPageCloseReasons_e)reason);
        }

        protected void OnClose(swPropertyManagerPageCloseReasons_e reason)
        {
            //This function must contain code, even if it does nothing, to prevent the
            //.NET runtime environment from doing garbage collection at the wrong time.

            if (reason ==  swPropertyManagerPageCloseReasons_e.swPropertyManagerPageClose_Okay)
            {
                MacroFeature.Commit();
            }else if (reason == swPropertyManagerPageCloseReasons_e.swPropertyManagerPageClose_Cancel)
            {
                MacroFeature.Cancel();
            }
        }

        public void AfterClose()
        {
            _Disposables?.ForEach(d=>d.Dispose());
            Page = null;
        }

        public virtual bool OnHelp()
        {
            return true;
        }

        public virtual bool OnPreviousPage()
        {
            return true;
        }

        public virtual bool OnNextPage()
        {
            return true;
        }

        public virtual bool OnPreview()
        {
            return true;
        }

        public virtual void OnWhatsNew()
        {
        }

        public virtual void OnUndo()
        {
        }

        public virtual void OnRedo()
        {
        }

        public virtual bool OnTabClicked(int id)
        {
            return true;
        }

        public virtual void OnGroupExpand(int id, bool expanded)
        {
        }

        public virtual void OnGroupCheck(int id, bool Checked)
        {
        }

        #region checkbox

        private readonly Subject<Tuple<int,bool>> _CheckBoxChanged = new Subject<Tuple<int,bool>>();
        public virtual void OnCheckboxCheck(int id, bool @checked)
        {
            _CheckBoxChanged.OnNext(Tuple.Create(id,@checked));
        }
        public IObservable<bool> CheckBoxChangedObservable(int id) => _CheckBoxChanged
            .Where(t=>t.Item1==id).Select(t=>t.Item2);
        #endregion

        private readonly Subject<int> _OptionChecked = new Subject<int>();
        public virtual void OnOptionCheck(int id)
        {
            _OptionChecked.OnNext(id);
        }

        public IObservable<int> OptionCheckedObservable(int id) => _OptionChecked.Where(i => i == id);

        public virtual void OnButtonPress(int id)
        {
        }

        #region textbox
        public virtual void OnTextboxChanged(int id, string text)
        {
            _TextBoxChanged.OnNext(Tuple.Create(id,text));
        }

        private readonly Subject<Tuple<int,string>> _TextBoxChanged = new Subject<Tuple<int,string>>();

        public IObservable<string> TextBoxChangedObservable(int id) => _TextBoxChanged
            .Where(t=>t.Item1==id).Select(t=>t.Item2);
        #endregion

        private List<IDisposable> _Disposables;

        #region numberbox

        private readonly Subject<Tuple<int,double>> _NumberBoxChanged = new Subject<Tuple<int,double>>();

        public IObservable<double> NumberBoxChangedObservable(int id) => _NumberBoxChanged
            .Where(t=>t.Item1==id).Select(t=>t.Item2);

        public virtual void OnNumberboxChanged(int id, double value)
        {
            _NumberBoxChanged.OnNext(Tuple.Create(id,value));
        }
        #endregion

        #region combobox
        public virtual void OnComboboxEditChanged(int id, string text)
        {
        }

        private readonly Subject<Tuple<int,int>> _ComboBoxSelectionSubject = new Subject<Tuple<int, int>>();
        public virtual void OnComboboxSelectionChanged(int id, int item)
        {
            _ComboBoxSelectionSubject.OnNext(Tuple.Create(id,item));
        }
        public IObservable<int> ComboBoxSelectionObservable(int id) => _ComboBoxSelectionSubject
            .Where(i => i.Item1 == id).Select(t => t.Item2);

        #endregion

        #region listbox

        private readonly Subject<Tuple<int,int>> _ListBoxSelectionSubject = new Subject<Tuple<int, int>>();
        private int _NextId = 0;

        public virtual void OnListboxSelectionChanged(int id, int item)
        {
            _ListBoxSelectionSubject.OnNext(Tuple.Create(id,item));
        }

        public IObservable<int> ListBoxSelectionObservable(int id) => _ListBoxSelectionSubject
            .Where(i => i.Item1 == id).Select(t => t.Item2);


        public virtual void OnListboxRMBUp(int id, int posX, int posY)
        {
        }
        #endregion

        public virtual void OnSelectionboxFocusChanged(int id)
        {
        }

        private Subject<int> _SelectionChangedSubject = new Subject<int>();
        public IObservable<Unit> SelectionChangedObservable(int id) => _SelectionChangedSubject.Where(i => id == i).Select(_=>Unit.Default); 
        public virtual void OnSelectionboxListChanged(int id, int count)
        {
            _SelectionChangedSubject.OnNext(id);
        }

        public virtual void OnSelectionboxCalloutCreated(int id)
        {
        }

        public virtual void OnSelectionboxCalloutDestroyed(int id)
        {
        }

        public bool OnSubmitSelection(int id, object selection, int selType, ref string itemText)
        {
            return OnSubmitSelection(id, selection, (swSelectType_e) selType, ref itemText );
        }

        protected virtual bool OnSubmitSelection(int id, object selection, swSelectType_e selType, ref string itemText)
        {
            return true;
        }


        public virtual int OnActiveXControlCreated(int id, bool status)
        {
            return -1;
        }

        public virtual void OnSliderPositionChanged(int id, double value)
        {
        }

        public virtual void OnSliderTrackingCompleted(int id, double value)
        {
        }

        public virtual bool OnKeystroke(int wparam, int message, int lparam, int id)
        {
            return true;
        }

        public virtual void OnPopupMenuItem(int id)
        {
        }

        public virtual void OnPopupMenuItemUpdate(int id, ref int retval)
        {
        }

        public virtual void OnGainedFocus(int id)
        {
        }

        public virtual void OnLostFocus(int id)
        {
        }

        public virtual int OnWindowFromHandleControlCreated(int id, bool status)
        {
            return 0;
        }


        public virtual void OnNumberBoxTrackingCompleted(int id, double value)
        {
        }

        protected IDisposable CreateListBox(IPropertyManagerPageGroup @group, string caption, string tip, Func<int> get, Action<int> set, Action<IPropertyManagerPageListbox> config)
        {
            var id = NextId();
            var list = PropertyManagerGroupExtensions.CreateListBox(@group, id, caption, tip);
            config(list);
            list.CurrentSelection = (short) get();
            return ListBoxSelectionObservable(id).Subscribe(set);
        }
        protected IDisposable CreateComboBox(IPropertyManagerPageGroup @group, string caption, string tip, Func<int> get, Action<int> set, Action<IPropertyManagerPageCombobox> config)
        {
            var id = NextId();
            var comboBox = PropertyManagerGroupExtensions.CreateComboBox(@group, id, caption, tip);
            config(comboBox);
            comboBox.CurrentSelection = (short) get();
            return ComboBoxSelectionObservable(id).Subscribe(set);
        }


        protected IDisposable CreateTextBox(IPropertyManagerPageGroup @group, string caption, string tip, Func<string> get, Action<string> set)
        {
            var id = NextId();
            var text = PropertyManagerGroupExtensions.CreateTextBox(@group, id, caption, tip);
            text.Text = get();
            return TextBoxChangedObservable(id).Subscribe(set);
        }

        protected IDisposable CreateCheckBox(IPropertyManagerPageGroup @group, string caption, string tip, Func<bool> get, Action<bool> set)
        {
            var id = NextId();
            var text = PropertyManagerGroupExtensions.CreateCheckBox(@group, id, caption, tip);
            text.Checked = get();
            return CheckBoxChangedObservable(id).Subscribe(set);
        }

        protected IDisposable CreateNumberBox(IPropertyManagerPageGroup @group, string tip, string caption, Func<double> get, Action<double> set, Action<IPropertyManagerPageNumberbox> config = null)
        {
            var id = NextId();
            var box = PropertyManagerGroupExtensions.CreateNumberBox(@group, id, caption, tip);
            box.Value = get();
            config?.Invoke(box);
            return NumberBoxChangedObservable(id).Subscribe(set);
        }
        protected IDisposable CreateLabel(IPropertyManagerPageGroup @group, string tip, string caption)
        {
            var id = NextId();
            var box = PropertyManagerGroupExtensions.CreateLabel(@group, id, caption, tip);
            return Disposable.Empty;
        }

        protected IDisposable CreateOption<T>(IPropertyManagerPageGroup @group, string tip, string caption, Func<T> get, Action<T> set, T match)
        {
            var id = NextId();
            if (match == null) throw new ArgumentNullException(nameof(match));

            var option = PropertyManagerGroupExtensions.CreateOption(@group, id, tip, caption);
            if (get().Equals(match))
            {
                option.Checked = true;
            }
            return OptionCheckedObservable(id).Subscribe(v=>set(match));
        }

        protected IDisposable CreateSelectionBox(IPropertyManagerPageGroup @group, string tip, string caption,
            Func<IPropertyManagerPageSelectionbox, IDisposable> config)
        {
            var id = NextId();
            var box = PropertyManagerGroupExtensions.CreateSelectionBox(@group, id, caption, tip);
            config(box);
            return Disposable.Empty;
        }

        protected IDisposable CreateSelectionBox(IPropertyManagerPageGroup @group, string tip, string caption,
            Action<IPropertyManagerPageSelectionbox> config)
        {
            var id = NextId();
            var box = PropertyManagerGroupExtensions.CreateSelectionBox(@group, id, caption, tip);
            config(box);
            // For the moment we don't have any callbacks / rx stuff to register.
            return Disposable.Empty;
        }


        private int NextId()
        {
            _NextId++;
            return _NextId;
        }
    }
}