using System;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.Sat;

namespace CSP_LeavePlanner
{
    class Season
    {
        private List<Employee> Employees { set; get; }
        private Dictionary<bool, List<AvailableDate>> Leaves { set; get; } //Leaves[true] holds 5day leaves, Leaves[false] holds 10day leaves
        private List<AvailableDate> AllLeaves { set; get; } //Holds all leave dates 5day ones first followed by 10day ones
        private TimeSpan ConflictPeriod;
        private bool[,] ConflictAll;
        private bool[,] Conflict5day;
        private bool[,] Conflict10day;

        public Season()
        {
            this.Employees = new List<Employee>();
            this.Leaves = new Dictionary<bool, List<AvailableDate>>();
            this.Leaves[true] = new List<AvailableDate>();
            this.Leaves[false] = new List<AvailableDate>();
            this.AllLeaves = new List<AvailableDate>();
            ConflictAll = null;
            Conflict5day = null;
            Conflict10day = null;
        }

        public void AddEmployee(Employee emp) //Add Employee to Season object
        {
            if (Employees.Exists(e => e.Id == emp.Id) == false)
                Employees.Add(emp);
            else
                throw new System.ArgumentException("Employee already exists", "emp");
        }

        public void AddLeave(AvailableDate date) //Add Leave date to Season object
        {
            bool isFiveDays = (date.end - date.start).Days == 4;

            AvailableDate leave = Leaves[isFiveDays].Find(l => l.Equals(date));
            if (leave != null)
                leave.availability++;
            else
                Leaves[isFiveDays].Add(date);
        }

        public void HandleInput(List<Employee> emps, List<AvailableDate> dates) //Handle all input
        {
            emps.ForEach(emp => AddEmployee(emp));
            dates.ForEach(dat => AddLeave(dat));
        }

        public void SeasonInfo()
        {
            Console.WriteLine("----- SEASON INFO -----");
            Console.WriteLine("----- EMPLOYEE INFO -----");
            Employees.ForEach(emp => emp.Print());
            Console.WriteLine("-------------------------");

            Console.WriteLine("-------- LEAVE INFO --------");
            Console.WriteLine("----- FIVE DAY LEAVES -----");
            Leaves[true].ForEach(dat => dat.Print());
            Console.WriteLine("---------------------------");
            Console.WriteLine("----- TEN DAY LEAVES -----");
            Leaves[false].ForEach(dat => dat.Print());
            Console.WriteLine("--------------------------");
            Console.WriteLine("----------------------------");
        }

        public void Init() //Initialization
        {
            AllLeaves.AddRange(Leaves[true]);
            AllLeaves.AddRange(Leaves[false]);

            ConflictPeriod = new TimeSpan(3 * 240, 0, 0);

            Employees.ForEach(e => e.TryAddChoice(Leaves));

            //Three type of conflict arrays
            //Between 5day leaves, 10day leaves and combined
            ConflictAll = AvailableDate.CreateRelationTable(AllLeaves, ConflictPeriod);
            Conflict5day = AvailableDate.CreateRelationTable(Leaves[true], ConflictPeriod);
            Conflict10day = AvailableDate.CreateRelationTable(Leaves[false], ConflictPeriod);
        }

        public void Solve()
        {
            //data.ForEach(d => d.Print());
            //Array.ForEach(employees, e => e.Print());
            //Console.ReadLine();

            /*for (int i = 0; i < Conflict10day.GetLength(0); i++)
            {
                for (int j = 0; j < Conflict10day.GetLength(1); j++)
                    Console.Write(String.Format("{0} ", Conflict10day[i, j]));
                Console.WriteLine();
            }*/

            CpModel model = new CpModel();

            Dictionary<int, List<IntVar>> Variables = new Dictionary<int, List<IntVar>>();
            List<IntVar> AllVariables = new List<IntVar>();

            //Lb means lower bound which is the smallest allowed value of a variable
            //Ub means upper bound which is the largest allowed value of a variable
            List<int> AllLbs = new List<int>();
            List<int> AllUbs = new List<int>();

            Employees.ForEach(e => {
                CalcLUBBasedOnLeaveMode(e, AllLeaves, AllLbs, AllUbs); //Calculates bounds of each employee's variables depending its type

                List<IntVar> vars = new List<IntVar>();
                for (int i = 0; i < e.NumDesiredDates; i++)
                {
                        IntVar des = model.NewIntVar(e.Lb[i], e.Ub[i], "Employee_" + e.Id + "_" + i); //Adds variables/decisions to the model
                        vars.Add(des);
                        AllVariables.Add(des);
                }
                Variables[e.Id] = vars;
            });

            //Console.WriteLine(data.Count);
            //Console.WriteLine(employees.Length);
            //Console.WriteLine(AllVariables.Count);
            //Console.ReadLine();

            for (int i = 0; i < AllLeaves.Count; i++) //Constrains each value to a maximum sum equal to the availability of the respective leave
            {
                List<IntVar> ValAppears = new List<IntVar>();
                for (int j = 0; j < AllVariables.Count; j++)
                {
                    if (AllLbs[j] > i || AllUbs[j] < i)
                        continue;

                    IntVar tmp = model.NewBoolVar("appears_" + i + "_" + j);
                    ValAppears.Add(tmp);

                    model.Add(AllVariables[j] == i).OnlyEnforceIf(tmp);
                    model.Add(AllVariables[j] != i).OnlyEnforceIf(tmp.Not());
                }

                model.AddSumConstraint(ValAppears, 0, AllLeaves.ElementAt(i).availability);
            }

            //model.AddAllDifferent(AllVariables.ToArray());

            int counter = 0;
            //IntVar[] prev_vars = null;
            Employees.ForEach(e => {
                IntVar[] vars = Variables[e.Id].ToArray();

                int c = 0;
                e.SetDates.ForEach(d => model.Add(vars[c++] == d.Id));
                //Console.WriteLine(c);
                //Console.ReadLine();

                //if (prev_vars != null)
                //model.Add(prev_vars.Sum() < vars.Sum());

                //for (int i=1; i<vars.Length; i++)
                //model.Add(vars[i-1] < vars[i]);

                //Adds conflict constraints between the variables of each employee depending on type
                if (e.LeaveType == Employee.LeaveMode.Days5)
                    AddConflictConstraints(model, e, vars, Conflict5day, 0, counter);
                else if (e.LeaveType == Employee.LeaveMode.Days10)
                    AddConflictConstraints(model, e, vars, Conflict10day, Leaves[true].Count, counter);
                else if (e.LeaveType == Employee.LeaveMode.Days10_5)
                    AddConflictConstraints(model, e, vars, ConflictAll, 0, counter);

                //prev_vars = vars;
            });

            //var sumTerm = new SumTermBuilder(AllVariables.Count);
            //AllVariables.ForEach(v => sumTerm.Add(v));
            //model.AddGoal("Goal", GoalKind.Minimize, Model.Sum(sumTerm.ToTerm()));

            //model.Minimize(AllVariables.ToArray().Sum());

            //Strategy of search
            model.AddDecisionStrategy(AllVariables, DecisionStrategyProto.Types.VariableSelectionStrategy.ChooseMinDomainSize,
                                                    DecisionStrategyProto.Types.DomainReductionStrategy.SelectMinValue);

            CpSolver solver = new CpSolver();

            //VarArraySolutionPrinter cb = new VarArraySolutionPrinter(AllVariables.ToArray());
            //solver.SearchAllSolutions(model, cb);
            //solver.SolveWithSolutionCallback(model, cb);
            CpSolverStatus status = solver.Solve(model);

            //Console.WriteLine(String.Format("Optimal objective value: {0}", cb.ObjectiveValue()));
            //Console.WriteLine(String.Format("Number of solutions found: {0}", cb.SolutionCount()));

            if (status == CpSolverStatus.Feasible)
            {
                Employees.ForEach(e => {
                    int i = 0;
                    foreach (var val in Variables[e.Id]) //updates Season with the results of the solver
                    {
                        if (i++ == 0 && e.InitialChoice != null)
                        {
                            AllLeaves.ElementAt((int)solver.Value(val)).availability--;
                            e.NumDesiredDates--;
                        }
                        else
                            e.AddDate(AllLeaves.ElementAt((int)solver.Value(val)));
                    }
                });

                Console.WriteLine("----- SOLVER INFO -----");
                Console.WriteLine(solver.WallTime());
                Console.WriteLine("-----------------------");

                Console.WriteLine("----- EMPLOYEE ASSIGNMENTS -----");
                Employees.ForEach(emp => emp.Print());
                Console.WriteLine("--------------------------------");

                Console.WriteLine("----- DATA INFO AFTER ASSIGMNENTS -----");
                Console.WriteLine("----- FIVE DAY LEAVES -----");
                Leaves[true].ForEach(dat => dat.Print());
                Console.WriteLine("---------------------------");
                Console.WriteLine("----- TEN DAY LEAVES -----");
                Leaves[false].ForEach(dat => dat.Print());
                Console.WriteLine("--------------------------");
                Console.WriteLine("---------------------------------------");
            }
            else if (status == CpSolverStatus.Infeasible)
            {
                Console.WriteLine("Model is Infeasible!");
            }
        }

        private void AddConflictConstraints(CpModel model, Employee e, IntVar[] vars, bool[,] ConflictArray, int offset, int counter)
        {
            for (int i = 0; i < ConflictArray.GetLength(0); i++)
            {
                for (int j = 0; j < ConflictArray.GetLength(1); j++)
                {
                    if (ConflictArray[i, j])
                        for (int k = 0; k < vars.Length; k++)
                            for (int l = k + 1; l < vars.Length; l++)
                            {
                                if ((e.Lb[k] > offset + i) || (e.Lb[l] > offset + j) || (e.Ub[k] < offset + i) || (e.Ub[l] < offset + j))
                                    continue;

                                IntVar a = model.NewBoolVar("a" + counter);
                                IntVar b = model.NewBoolVar("b" + counter);

                                model.Add(vars[k] == offset+i).OnlyEnforceIf(a);
                                model.Add(vars[k] != offset+i).OnlyEnforceIf(a.Not());

                                model.Add(vars[l] != offset+j).OnlyEnforceIf(b);
                                model.Add(vars[l] == offset+j).OnlyEnforceIf(b.Not());

                                model.AddImplication(a, b);
                            }
                }
            }
        }

        private void CalcLUBBasedOnLeaveMode(Employee e, List<AvailableDate> allLeaves, List<int> allLbs, List<int> allUbs)
        {
            e.Lb = new int[e.NumDesiredDates];
            e.Ub = new int[e.NumDesiredDates];

            if (e.LeaveType == Employee.LeaveMode.Days5)
            {
                for (int i = 0; i < e.NumDesiredDates; i++)
                {
                    e.Lb[i] = 0;
                    e.Ub[i] = Leaves[true].Count - 1;
                    allLbs.Add(0);
                    allUbs.Add(Leaves[true].Count - 1);
                }
            }
            else if (e.LeaveType == Employee.LeaveMode.Days10)
            {
                for (int i = 0; i < e.NumDesiredDates; i++)
                {
                    e.Lb[i] = Leaves[true].Count;
                    e.Ub[i] = allLeaves.Count - 1;
                    allLbs.Add(Leaves[true].Count);
                    allUbs.Add(allLeaves.Count - 1);
                }
            }
            else if (e.LeaveType == Employee.LeaveMode.Days10_5)
            {
                e.Lb[0] = Leaves[true].Count;
                e.Ub[0] = allLeaves.Count - 1;
                allLbs.Add(Leaves[true].Count);
                allUbs.Add(allLeaves.Count - 1);

                for (int i = 1; i < e.NumDesiredDates; i++)
                {
                    e.Lb[i] = 0;
                    e.Ub[i] = Leaves[true].Count - 1;
                    allLbs.Add(0);
                    allUbs.Add(Leaves[true].Count - 1);
                }
            }
        }
    }

    public class VarArraySolutionPrinter : CpSolverSolutionCallback
    {
        public VarArraySolutionPrinter(IntVar[] variables)
        {
            variables_ = variables;
        }

        public override void OnSolutionCallback()
        {
            {
                Console.WriteLine(String.Format("Solution #{0}: time = {1:F2} s",
                                                solution_count_, WallTime()));
                foreach (IntVar v in variables_)
                {
                    Console.WriteLine(
                        String.Format("  {0} = {1}", v.ShortString(), Value(v)));
                }
                solution_count_++;

                Console.ReadLine();
            }
        }

        public int SolutionCount()
        {
            return solution_count_;
        }

        private int solution_count_;
        private IntVar[] variables_;
    }
}