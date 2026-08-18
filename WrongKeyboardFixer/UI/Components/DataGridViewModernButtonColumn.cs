using System;
using System.Windows.Forms;

namespace WrongKeyboardFixer.UI.Components;

/// <summary>
/// A DataGridView column that uses DataGridViewModernButtonCell.
/// </summary>
public class DataGridViewModernButtonColumn : DataGridViewButtonColumn
{
    private int _cornerRadius = 7;

    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int CornerRadius
    {
        get => _cornerRadius;
        set 
        { 
            _cornerRadius = value; 
            if (CellTemplate is DataGridViewModernButtonCell cell)
            {
                cell.CornerRadius = value;
            }
        }
    }

    public DataGridViewModernButtonColumn() : base()
    {
        CellTemplate = new DataGridViewModernButtonCell();
    }
}