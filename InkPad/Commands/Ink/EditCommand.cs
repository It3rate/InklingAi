namespace InkPad.Commands.Ink;

using InkPad.CommandEngine;
using InkPad.Elements;
using InkPad.Pads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class EditCommand : InkCommandBase
{
    new CommandStack Stack { get; set; }
    public EditCommand(Pad pad) : base(pad) { }
}
