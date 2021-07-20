using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace AloysAdjustments.Common.UI.Controls
{
    public class AutoSelectTextBox : TextBox
    {
        private bool _autoSelectAll = true;

        protected override void OnInitialized(EventArgs e)
        {
            // This will cause the cursor to enter the text box ready to
            // type even when there is no content.
            Focus();
            base.OnInitialized(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // This is here to handle the case of an empty text box.  If
            // omitted then the first character would be auto selected when
            // the user starts typing.
            _autoSelectAll = false;
            base.OnKeyDown(e);
        }


        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            if (_autoSelectAll)
            {
                SelectAll();
                Focus();
                _autoSelectAll = false;
            }
            base.OnTextChanged(e);
        }
    }
}
