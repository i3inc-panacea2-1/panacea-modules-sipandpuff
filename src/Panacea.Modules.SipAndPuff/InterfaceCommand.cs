using Panacea.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Input;

namespace Panacea.Modules.SipAndPuff
{
    internal class InterfaceCommand : INotifyPropertyChanged
    {
        public event EventHandler<Tree<InterfaceCommand>> Inspect;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public InterfaceCommand(AutomationElement element)
        {
            Element = element;
            UpdateTexts();
            Command = new RelayCommand(async (parameter) =>
            {
                if ((parameter as Tree<InterfaceCommand>).IsLeaf)
                {
                    await Task.Run(async() =>
                    {
                        try
                        {
                            Console.WriteLine(element.Current.ControlType.ProgrammaticName);
                            if (element.Current.ControlType == ControlType.Button)
                            {
                                var com = element.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                                com.Invoke();
                            }
                            else if (element.Current.ControlType == ControlType.ListItem)
                            {
                                var com = element.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
                                com.Select();
                            }

                            await Task.Delay(300);
                            await UpdateTexts();
                        }
                        catch (ElementNotAvailableException)
                        {

                        }
                    });
                   
                }
                else
                {
                    Console.WriteLine("Inspect");
                    Inspect?.Invoke(this, (parameter as Tree<InterfaceCommand>));
                }
            });
        }

        Task UpdateTexts()
        {
            return Task.Run(() =>
            {
                try
                {
                    var text = GetText(Element);
                    Label = (!string.IsNullOrEmpty(text) ? text : Element.Current.ControlType.ProgrammaticName)
                        + Environment.NewLine
                        + "(" + string.Join(",", Element.GetRuntimeId()) + ")";
                    Description = Element.Current.HelpText;
                }
                catch { }
            });
           
        }

        

        public static string GetText(AutomationElement element)
        {
            object patternObj;
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out patternObj))
            {
                var valuePattern = (ValuePattern)patternObj;
                return valuePattern.Current.Value;
            }
            else if (element.TryGetCurrentPattern(TextPattern.Pattern, out patternObj))
            {
                var textPattern = (TextPattern)patternObj;
                return textPattern.DocumentRange.GetText(-1).TrimEnd('\r'); // often there is an extra '\r' hanging off the end.
            }
            else
            {
                return element.Current.Name;
            }
        }


        public AutomationElement Element { get; private set; }
        public ICommand Command { get; set; }

        string _label;
        public string Label
        {
            get => _label;
            set
            {
                _label = value;
                OnPropertyChanged();
            }
        }

        string _description;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

    }
}
