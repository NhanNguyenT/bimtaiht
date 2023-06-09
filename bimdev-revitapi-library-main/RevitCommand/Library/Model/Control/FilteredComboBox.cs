﻿namespace Model.Control
{
    using System.Collections;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Linq;
    using System;
    using SingleData;

    /// <summary>
    /// Editable combo box which uses the text in its editable textbox to perform a lookup
    /// in its data source.
    /// </summary>
    public class FilteredComboBox : ComboBox
    {
        protected Func<object, bool> filterObjectFunc = x => true;
        protected Func<object, bool>? filterTextSearchFunc;

        private FormData formData => FormData.Instance;

        /// <summary>
        /// The search string treshold length.
        /// </summary>
        /// <remarks>
        /// It's implemented as a Dependency Property, so you can set it in a XAML template 
        /// </remarks>
        public static readonly DependencyProperty MinimumSearchLengthProperty =
            DependencyProperty.Register(
                "MinimumSearchLength",
                typeof(int),
                typeof(FilteredComboBox),
                new UIPropertyMetadata(1));

        ////
        // Private Fields
        //// 

        /// <summary>
        /// Caches the previous value of the filter.
        /// </summary>
        private string oldFilter = string.Empty;

        /// <summary>
        /// Holds the current value of the filter.
        /// </summary>
        private string currentFilter = string.Empty;

        ////
        // Properties
        //// 

        /// <summary>
        /// Gets or sets the search string treshold length.
        /// </summary>
        /// <value>The minimum length of the search string that triggers filtering.</value>
        [Description("Length of the search string that triggers filtering.")]
        [Category("Filtered ComboBox")]
        [DefaultValue(3)]
        public int MinimumSearchLength
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return (int)this.GetValue(MinimumSearchLengthProperty);
            }

            [System.Diagnostics.DebuggerStepThrough]
            set
            {
                this.SetValue(MinimumSearchLengthProperty, value);
            }
        }

        /// <summary>
        /// Gets a reference to the internal editable textbox.
        /// </summary>
        /// <value>A reference to the internal editable textbox.</value>
        /// <remarks>
        /// We need this to get access to the Selection.
        /// </remarks>
        protected TextBox EditableTextBox
        {
            get
            {
                return (this.GetTemplateChild("PART_EditableTextBox") as TextBox)!;
            }
        }

        ////
        // Event Raiser Overrides
        //// 

        /// <summary>
        /// Keep the filter if the ItemsSource is explicitly changed.
        /// </summary>
        /// <param name="oldValue">The previous value of the filter.</param>
        /// <param name="newValue">The current value of the filter.</param>
        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (newValue != null)
            {
                ICollectionView view = CollectionViewSource.GetDefaultView(newValue);
                view.Filter += this.FilterPredicate;
            }

            if (oldValue != null)
            {
                ICollectionView view = CollectionViewSource.GetDefaultView(oldValue);
                view.Filter -= this.FilterPredicate;
            }

            base.OnItemsSourceChanged(oldValue, newValue);
        }

        /// <summary>
        /// Confirm or cancel the selection when Tab, Enter, or Escape are hit. 
        /// Open the DropDown when the Down Arrow is hit.
        /// </summary>
        /// <param name="e">Key Event Args.</param>
        /// <remarks>
        /// The 'KeyDown' event is not raised for Arrows, Tab and Enter keys.
        /// It is swallowed by the DropDown if it's open.
        /// So use the Preview instead.
        /// </remarks>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Tab || e.Key == Key.Enter || e.Key == Key.System)
            {
                // Explicit Selection -> Close ItemsPanel
                this.IsDropDownOpen = false;
            }
            else if (e.Key == Key.Escape)
            {
                // Escape -> Close DropDown and redisplay Filter
                this.IsDropDownOpen = false;
                this.SelectedIndex = -1;
                this.Text = this.currentFilter;
            }
            else
            {
                if (e.Key == Key.Down)
                {
                    // Arrow Down -> Open DropDown
                    this.IsDropDownOpen = true;
                }

                base.OnPreviewKeyDown(e);
            }

            // Cache text
            this.oldFilter = this.Text;
        }

        /// <summary>
        /// Modify and apply the filter.
        /// </summary>
        /// <param name="e">Key Event Args.</param>
        /// <remarks>
        /// Alternatively, you could react on 'OnTextChanged', but navigating through 
        /// the DropDown will also change the text.
        /// </remarks>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                // Navigation keys are ignored
            }
            else if (e.Key == Key.Tab || e.Key == Key.Enter)
            {
                // Explicit Select -> Clear Filter
                this.ClearFilter();
            }
            else
            {
                // The text was changed
                if (this.Text != this.oldFilter)
                {
                    // Clear the filter if the text is empty,
                    // apply the filter if the text is long enough
                    if (this.Text.Length == 0 || this.Text.Length >= this.MinimumSearchLength)
                    {
                        this.RefreshFilter();
                        this.IsDropDownOpen = true;

                        // Unselect
                        try
                        {
                            if (this.EditableTextBox != null)
                            {
                                this.EditableTextBox.SelectionStart = int.MaxValue;
                            }
                        }
                        catch(System.NullReferenceException)
                        {
                            throw;
                        }
                    }
                }

                //base.OnKeyUp(e);

                // Update Filter Value
                this.currentFilter = this.Text;
            }
        }

        /// <summary>
        /// Make sure the text corresponds to the selection when leaving the control.
        /// </summary>
        /// <param name="e">A KeyBoardFocusChangedEventArgs.</param>
        protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            this.ClearFilter();
            //int temp = this.SelectedIndex;
            //this.SelectedIndex = -1;
            //this.Text = string.Empty;
            //this.SelectedIndex = temp;
            base.OnPreviewLostKeyboardFocus(e);
        }

        ////
        // Helpers
        ////

        /// <summary>
        /// Re-apply the Filter.
        /// </summary>
        private void RefreshFilter()
        {
            if (this.ItemsSource != null)
            {
                var isSameText = false;
                object selectedItem = this.SelectedItem;
                var text = this.Text;

                if (text != null && selectedItem != null)
                {
                    if (this.Text == this.SelectedItem.ToString())
                    {
                        selectedItem = this.SelectedItem;
                        isSameText = true;
                    }
                }

                ICollectionView view = CollectionViewSource.GetDefaultView(this.ItemsSource);
                view.Refresh();

                if (isSameText)
                {
                    this.SelectedItem = selectedItem;
                }
                else
                {
                    this.SelectedItem = null;
                    this.Text = text;
                }
            }
        }

        /// <summary>
        /// Clear the Filter.
        /// </summary>
        private void ClearFilter()
        {
            this.currentFilter = string.Empty;
            this.RefreshFilter();
        }

        /// <summary>
        /// The Filter predicate that will be applied to each row in the ItemsSource.
        /// </summary>
        /// <param name="value">A row in the ItemsSource.</param>
        /// <returns>Whether or not the item will appear in the DropDown.</returns>
        protected bool FilterPredicate(object value)
        {
            if (SelectedItem != null && Text != null)
            {
                if (SelectedItem.ToString() == Text)
                {
                    return true;
                }
            }

            // No filter, no text
            if (value == null)
            {
                return false;
            }

            // No text, no filter
            if (this.Text == null || this.Text.Length == 0)
            {
                return filterObjectFunc(value);
            }

            if (filterTextSearchFunc != null)
            {
                return filterObjectFunc(value) && filterTextSearchFunc(value);
            }
            else
            {
                var textValue = this.Text.ToLower();
                var objValue = value.ToString().ToLower();

                return objValue.Contains(textValue);
            }
        }

        protected override void OnDropDownOpened(EventArgs e)
        {
            formData.IsDropDownOpen = true;
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            formData.IsDropDownOpen = false;
        }
    }
}