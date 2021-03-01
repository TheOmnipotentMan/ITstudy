using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// TODO remove if the app is ever finished
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ITstudii.RedProjects
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Calculator : Page
    {

        double FirstNumber;
        double SecondNumber;
        double CalculationResult;

        string FirstNumberInputString;
        string SecondNumberInputString;

        enum CalculatorOperators { Plus, Minus, Multiply, Divide, Power, Root }
        CalculatorOperators ActiveOperator;


        public Calculator()
        {
            this.InitializeComponent();
        }

        // Do the calculation that has been entered
        private void DoCalculation_Click(object sender, RoutedEventArgs e)
        {
            // fill in the neccesary fields, if all goes well, do the calculation
            if (SetNumberInput())
            {
                Debug.WriteLine("Calculator input is all valid");
                // catch any problematic calculations, maybe also possible with try {} catch (DivideByZeroException)
                if ((ActiveOperator == CalculatorOperators.Divide) && (SecondNumber == 0)) { ResultErrorDisplay.Text = "Divide by zero"; CalculationResultDisplay.Text = "∞"; }
                else if ((ActiveOperator == CalculatorOperators.Root) && (FirstNumber == 0)) { ResultErrorDisplay.Text = "Can't calculate"; CalculationResultDisplay.Text = "∞"; }

                else { 
                    switch (ActiveOperator)
                    {
                        case CalculatorOperators.Plus: { CalculationResult = FirstNumber + SecondNumber; 
                                CalculationResultDisplay.Text = CalculationResult.ToString(); }
                            break;
                        case CalculatorOperators.Minus: { CalculationResult = FirstNumber - SecondNumber;
                                CalculationResultDisplay.Text = CalculationResult.ToString(); } 
                            break;
                        case CalculatorOperators.Multiply: { CalculationResult = FirstNumber * SecondNumber; 
                                CalculationResultDisplay.Text = CalculationResult.ToString(); } 
                            break;
                        case CalculatorOperators.Divide: { CalculationResult = FirstNumber / SecondNumber;
                                CalculationResultDisplay.Text = CalculationResult.ToString(); }
                            break;
                        case CalculatorOperators.Power: { CalculationResult = Math.Pow(FirstNumber, SecondNumber);
                                CalculationResultDisplay.Text = CalculationResult.ToString(); }
                            break;
                        case CalculatorOperators.Root: { CalculationResult = Math.Pow(SecondNumber, 1 / FirstNumber); 
                                CalculationResultDisplay.Text = CalculationResult.ToString(); }
                            break;
                        default: { ResultErrorDisplay.Text = "Error"; } break;
                    }
                }
            }
        }

        private bool SetNumberInput()
        {
            // clear any previous messages from UI
            FirstNumberErrorDisplay.Text = String.Empty;
            OperatorErrorDisplay.Text = String.Empty;   // OperatorErrorDisplay may not be needed
            SecondNumberErrorDisplay.Text = String.Empty;
            ResultErrorDisplay.Text = String.Empty;
            CalculationResultDisplay.Text = String.Empty;

            // make sure the input is readable
            try { FirstNumberInput.Document.GetText(Windows.UI.Text.TextGetOptions.AdjustCrlf, out FirstNumberInputString); }
            catch { FirstNumberErrorDisplay.Text = "Could not read"; return false; }
            if ((ComboBoxItem)OperatorInput.SelectedItem == null) { ChangeCurrentOperator(); }
            try { SecondNumberInput.Document.GetText(Windows.UI.Text.TextGetOptions.AdjustCrlf, out SecondNumberInputString); }
            catch { SecondNumberErrorDisplay.Text = "Could not read"; return false; }

            // default to 1 if no input given
            if ( FirstNumberInputString == String.Empty ) { FirstNumberInputString = "1"; }
            if ( SecondNumberInputString == String.Empty ) { SecondNumberInputString = "1"; }
            Debug.WriteLine(string.Format("FirstNumberInput: {0}", FirstNumberInputString));    // 'string.Format' is included because of overload
            Debug.WriteLine(string.Format("SecondNumberInput: {0}", SecondNumberInputString));

            // convert input from string to double
            try { FirstNumber = Convert.ToDouble(FirstNumberInputString); }
            catch { FirstNumberErrorDisplay.Text = "Can't read Number"; return false; }
            try { SecondNumber = Convert.ToDouble(SecondNumberInputString); }
            catch { SecondNumberErrorDisplay.Text = "Can't read Number"; return false; }
            Debug.WriteLine("FirstNumber: {0}", FirstNumber);
            Debug.WriteLine("SecondNumber: {0}", SecondNumber);


            // if all goes well
            return true;
        }



        // register selection change in Operator
        private void OperatorChange(object sender, SelectionChangedEventArgs e)
        {
            ChangeCurrentOperator();
        }

        // change the selected operator based on (ComboBox)OperatorInput selection
        private void ChangeCurrentOperator()
        {
            if ((ComboBoxItem)OperatorInput.SelectedItem == null) { OperatorInput.SelectedItem = (ComboBoxItem)OperatorInput.Items.First(); }
            string OperatorSelectedName = ((ComboBoxItem)OperatorInput.SelectedItem).Name;
            OperatorNameDisplay.Text = OperatorSelectedName;

            switch (OperatorSelectedName)
            {
                case "Plus": ActiveOperator = CalculatorOperators.Plus; break;
                case "Minus": ActiveOperator = CalculatorOperators.Minus; break;
                case "Multiply": ActiveOperator = CalculatorOperators.Multiply; break;
                case "Divide": ActiveOperator = CalculatorOperators.Divide; break;
                case "Power": ActiveOperator = CalculatorOperators.Power; break;
                case "Root": ActiveOperator = CalculatorOperators.Root; break;
                default: OperatorNameDisplay.Text = "Error"; break;
            }

            Debug.WriteLine("OperatorItem selected; {0}. ActiveOperator set to; {1}", OperatorSelectedName, ActiveOperator.ToString());
        }
    }
}
