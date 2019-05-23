using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Compiler
{
    public class Variable
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public Variable(string name, int value = 0)
        {
            Name = name;
            Value = value;
        }
    }
    class Program
    {       
        static void Main(string[] args)
        { 
            // List to store all the lines of code
            List<string> lines = new List<string>();
            // List to store all the declared variables
            List<Variable> variables = new List<Variable>();
            // List to store the assembly instructions
            List<string> assembly = new List<string>();
            // List to store all the erros that arise
            List<string> errors = new List<string>();

            int lineNo = 0;
            string outputLine = "";
            string input = "";
            bool fin = false;
            Variable workingVar;
            string[] segments;
            bool found = false;
            int numErrors = 0;
            int numIFstatements = 0;
            bool ifstatement = false;
            int numloops = 0;
            int numindents = 0;

            // Load the BASIC into the list
            while (!fin)
            {
                Console.Write("{0}:", lineNo);
                for(int i = 1; i <= numindents; i++)
                {
                    // Add the tabs for if statements and while loops. Unsure if it actually helps readability, may remove.
                    Console.Write("\t");
                }
                input = Console.ReadLine();
                if (input == "COMPILE")
                {
                    // When 'COMPILE' is entered, it means that the program should stop accepting input and start processing
                    fin = true;
                }
                else
                {
                    lines.Add(input);
                    segments = input.Split(' ');
                    lineNo++;
                    switch(segments[0])
                    {
                        case "while":
                        case "if":
                            numindents++;
                            break;
                        case "endwhile":
                        case "endif":
                            numindents--;
                            break;
                    }
                }
            }
            //---------------------------
            lineNo = 0;
            // Process the BASIC
            foreach (string line in lines)
            {          
                segments = line.Split(' ');

                // Find the variables
                workingVar = Findvar(line);
                if (workingVar.Name != "N_F")
                {
                    variables.Add(workingVar);
                }

                //Find all the commands

                switch (segments[0])
                {
                    case "print":
                        if (segments.Length == 2)
                        {
                            found = false;
                            foreach (Variable item in variables)
                            {
                                //Search through the variables array to find the variable value that it wants
                                if (item.Name == segments[1])
                                {
                                    assembly.Add("LDA " + item.Name);
                                    assembly.Add("OUT");
                                    found = true;
                                }
                            }
                            if (!found)
                            {
                                errors.Add("Error on line " + lineNo + " could not find " + segments[1]);
                                numErrors += 1;
                            }
                        }
                        else
                        {
                            errors.Add("Error on line " + lineNo + ", syntax incorrect");
                            numErrors += 1;
                        }
                        break;
                    case "input":
                        if(segments.Length == 2)
                        {
                            found = false;
                            foreach (Variable thing in variables)
                            {
                                if (segments[1] == thing.Name)
                                {
                                    found = true;
                                    assembly.Add("INP");
                                    assembly.Add("STA " + segments[1]);
                                }
                            }
                            if (!found)
                            {
                                numErrors += 1;
                                errors.Add("could not find variable on input");
                            }
                        }
                        else
                        {
                            numErrors += 1;
                            errors.Add("Could not compile: error on line " + lineNo.ToString());
                        }
                        break;
                    case "end":
                        assembly.Add("HLT");
                        break;
                    case "while":
                        // create a while loop with a condition that gets checked at the start and will always branch to the start
                        string first = "";
                        string second = "";
                        //Create entry condition
                        if (segments.Length == 4) 
                        {
                            //Find variables in list
                            foreach (Variable item in variables)
                            {
                                if (item.Name == segments[1])
                                {
                                    first = item.Name;
                                }
                                if (item.Name == segments[3])
                                {
                                    second = item.Name;
                                }
                            }
                            if(first != "" && second != "" && lines.Contains("endwhile") && segments[2] == "!=")
                            {
                                assembly.Add("lstart" + numloops + " LDA " + first);
                                assembly.Add("SUB " + second);
                                assembly.Add("BRZ " + "outloop" + numloops);
                                assembly.Add("BRA " + "loop" + numloops);
                                assembly.Add("loop" + numloops + " LDA 00");
                                numloops += 1;
                            }
                            else if (first != "" && second != "" && lines.Contains("endwhile") && segments[2] == "=")
                            {
                                assembly.Add("lstart" + numloops + " LDA " + first);
                                assembly.Add("SUB " + second);
                                assembly.Add("BRZ " + "loop" + numloops);
                                assembly.Add("BRA " + "outloop" + numloops);
                                assembly.Add("loop" + numloops + " LDA 00");
                                numloops += 1;
                            }
                            else if(first != "" && second != "" && lines.Contains("endwhile") && segments[2] == ">=")
                            {
                                assembly.Add("lstart" + numloops + " LDA " + first);
                                assembly.Add("SUB " + second);
                                assembly.Add("BRP " + "loop" + numloops);
                                assembly.Add("BRA " + "outloop" + numloops);
                                assembly.Add("loop" + numloops + " LDA 00");
                                numloops += 1;
                            }

                        }
                        break;
                    case "endwhile":
                        assembly.Add("BRA " + "lstart" + (numloops - 1));
                        assembly.Add("outloop" + (numloops - 1) + " LDA 00");
                        numloops -= 1;
                        break;
                    case "if":
                        string var1 = "";
                        string var2 = "";
                        if (segments.Length == 4)
                        {
                            //Find variables in list
                            foreach (Variable item in variables)
                            {
                                if (item.Name == segments[1])
                                {
                                    var1 = item.Name;
                                }
                                if (item.Name == segments[3])
                                {
                                    var2 = item.Name;
                                }
                            }
                            if (var1 != "" && var2 != "" && lines.Contains("endif") && segments[2] == "=")
                            {
                                assembly.Add("LDA " + var1);
                                assembly.Add("SUB " + var2);
                                assembly.Add("BRZ " + "TRUE" + numIFstatements.ToString());
                                if (lines.Contains("else"))
                                {
                                    assembly.Add("BRA " + "ELSE" + numIFstatements.ToString());
                                }
                                else
                                {
                                    assembly.Add("BRA " + "RCODE" + numIFstatements.ToString());
                                }
                                assembly.Add("TRUE" + numIFstatements + " LDA 00"); // Having LDA 00 is a placeholder and gets optimised out
                                ifstatement = true;
                            }
                            else if(var1 != "" && var2 != "" && lines.Contains("endif") && segments[2] == ">=")
                            {
                                assembly.Add("LDA " + var1);
                                assembly.Add("SUB " + var2);
                                assembly.Add("BRP " + "TRUE" + numIFstatements.ToString());
                                if (lines.Contains("else"))
                                {
                                    assembly.Add("BRA " + "ELSE" + numIFstatements.ToString());
                                }
                                else
                                {
                                    assembly.Add("BRA " + "RCODE" + numIFstatements.ToString());
                                }
                                assembly.Add("TRUE" + numIFstatements + " LDA 00"); // Having LDA 00 is a placeholder and gets optimised out
                                ifstatement = true;
                            }
                            else
                            {
                                numErrors += 1;
                                errors.Add("Error on line " + lineNo.ToString() + " Bad IF statement");
                            }


                        }
                        break;
                    case "endif":
                        assembly.Add("RCODE" + numIFstatements + " LDA 00");
                        numIFstatements += 1;
                        ifstatement = false;
                        break;
                    case "else":
                        if(ifstatement == true)
                        {
                            assembly.Add("BRA " + "RCODE" + numIFstatements);
                            assembly.Add("ELSE" + numIFstatements + " LDA 00");
                        }
                        else
                        {
                            errors.Add("Error on line " + lineNo.ToString() + " BAD ELSE");
                            numErrors += 1;
                        }
                        break;
                        
                    default:
                        // Search for variables for which maths can be performed. 
                        found = false;
                        foreach(Variable item in variables)
                        {
                            if(item.Name == segments[0])
                            {
                                //Variable found!
                                if(segments.Length == 3)
                                {
                                    if(segments[1] == "=")
                                    {
                                        //This means that there is an assignment requested!
                                        //We'll follow the standard tradition of going from left<-right

                                        //Find variable at segments[2]
                                        foreach (Variable thing in variables)
                                        {
                                            if (segments[2] == thing.Name)
                                            {
                                                found = true;
                                                assembly.Add("LDA " + thing.Name);
                                                assembly.Add("STA " + item.Name);
                                            } 
                                        }
                                    }
                                    else
                                    {
                                        errors.Add("Invalid syntax on line " + lineNo);
                                    }

                                }

                                else if (segments.Length == 5)
                                {
                                    //if the length is 5 that probably means something in the structure A = A + B
                                    if (segments[1] == "=")
                                    {
                                        string subvar1 = "";
                                        string subvar2 = "";
                                        foreach (Variable thing in variables)
                                        {
                                            if (segments[2] == thing.Name)
                                            {
                                                subvar1 = thing.Name;
                                            }
                                            if (segments[4] == thing.Name)
                                            {
                                                subvar2 = thing.Name;
                                            }
                                        }
                                        if (subvar1 != "" && subvar2 != "")
                                        {
                                            found = true;
                                            switch (segments[3])
                                            {
                                                case "+":
                                                    assembly.Add("LDA " + subvar1);
                                                    assembly.Add("ADD " + subvar2);
                                                    assembly.Add("STA " + segments[0]);
                                                    break;
                                                case "-":
                                                    assembly.Add("LDA " + subvar1);
                                                    assembly.Add("SUB " + subvar2);
                                                    assembly.Add("STA " + segments[0]);
                                                    break;
                                                default:
                                                    errors.Add("Error on line " + lineNo + ": bad math operation");
                                                    break;
                                            }        
                                        }

                                    }
                                }
                                else
                                {
                                    errors.Add("Invalid syntax on line " + lineNo);
                                    numErrors += 1;
                                }
                            }
                           
                        }
                        if (!found && segments[0] != "int")
                        {
                            numErrors += 1;
                            errors.Add("Could not compile. Error on line " + (lineNo));
                        }
                        break;     
                }
                lineNo += 1;
            }
            // [LAST SEGMENT] Generate the variable declarations
            foreach(Variable item in variables)
            {
                outputLine = item.Name + " DAT " + item.Value.ToString();
                assembly.Add(outputLine);
            }

            
            //Show the errors to the screen
            if (numErrors == 0)
            {
                int count = 0;
                string[] parts;
                string exepath = AppDomain.CurrentDomain.BaseDirectory;
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(exepath, "AssemblyOutput.txt")))
                {
                    while (count <= assembly.Count - 1)
                    {
                        if (assembly[count].Contains("LDA 00") && !(assembly[count + 1].Contains("lstart")))
                        {
                            // LDA 00 is a placeholder as it knows it needs to place a label but not what the next instruction will be
                            // As LDA 00 is useless and is a waste, we optimise this out.
                            parts = assembly[count].Split(' ');
                            Console.WriteLine(parts[0] + " " + assembly[count + 1]);
                            outputFile.WriteLine(parts[0] + " " + assembly[count + 1]);
                            count += 1;
                        }
                        else
                        {
                            Console.WriteLine(assembly[count]);
                            outputFile.WriteLine(assembly[count]);
                        }
                        count += 1;
                    }
                }
                    
            }
            else
            {
                Console.WriteLine("Could not compile. There were {0} errors:", numErrors);
                foreach (string error in errors)
                {
                    Console.WriteLine(error);
                }

            }
            Console.ReadLine();
        }

        //-----------------------------------
        
        // Subroutines
        static Variable Findvar(string line)
        {
            // Searches through all of the lines of code given, and tries to find 'int'.
            // It will only search for integers as LMC only accepts integers.
            Variable workingVar;
            string[] segments; // Array to store all parts of each line
            segments = line.Split(' '); // Split the statements by a space
            if(segments.Length >= 2) 
            {
                if (segments[0] == "int" && segments[1] != "")
                {
                    // Integer declaration found!
                    // Store the identifier in another list for safe-keeping.
                    workingVar = new Variable(segments[1]);
                    if (segments.Length == 4)
                    {
                        workingVar.Value = int.Parse(segments[3]);
                    }
                    return workingVar;
                }  
            }
            workingVar = new Variable("N_F");
            return workingVar;
        }
        //------------------------------------
        static public Boolean IsNumber(String value)
        {
            return value.All(Char.IsDigit);
        }
        
        
    }
        
}
