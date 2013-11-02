using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drawing
{

    public delegate void EditAction();

    public delegate bool IsEditActive();

    /// <summary>
    /// Definition for displaying edit actions
    /// </summary>
    public class EditDefinition
    {
        internal readonly string Name;

        internal readonly EditAction Action;

        internal readonly IsEditActive IsActive;

        public EditDefinition(string name, EditAction action, IsEditActive isActive)
        {
            Name=name;
            Action = action;
            IsActive = isActive;
        }
    }
}
