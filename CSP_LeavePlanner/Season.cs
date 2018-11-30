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

        private List<int> AllLbs;
        private List<int> AllUbs;


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
            AllLbs = null;
            AllUbs = null;
        }

        private static TimeSpan ts30days = new TimeSpan(days: 30, hours: 0, minutes: 0, seconds: 0);

        public void AddEmployee(Employee emp) //Add Employee to Season object
        {
            if (Employees.Exists(e => e.Id == emp.Id) == false)
                Employees.Add(emp);
            else
                throw new ArgumentException("Employee already exists", "emp");
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

            ConflictPeriod = ts30days;

            AddInitialEmployeeChoices();

            //Three type of conflict arrays
            //Between 5day leaves, 10day leaves and combined
            ConflictAll = AvailableDate.CreateRelationTable(AllLeaves, ConflictPeriod);
            Conflict5day = AvailableDate.CreateRelationTable(Leaves[true], ConflictPeriod);
            Conflict10day = AvailableDate.CreateRelationTable(Leaves[false], ConflictPeriod);

            //CalculateAndAdjustNeeds();

            //Lb means lower bound which is the smallest allowed value of a variable
            //Ub means upper bound which is the largest allowed value of a variable
            AllLbs = new List<int>();
            AllUbs = new List<int>();

            foreach (var e in Employees)
                CalcLUBBasedOnLeaveMode(e, AllLeaves, AllLbs, AllUbs); //Calculates bounds of each employee's variables depending on its type
        }

        private void AddInitialEmployeeChoices()
        {
            Dictionary<String, int> frequency = new Dictionary<string, int>();
            AvailableDate choice;
            Employees.ForEach(e => {
                if (e.InitialChoice != null)
                {
                    if (frequency.ContainsKey(e.InitialChoice))
                    {
                        if (frequency[e.InitialChoice] < (choice = AvailableDate.GetDateByName(AllLeaves, e.InitialChoice)).availability)
                        {
                            if (e.TryAddChoice(choice))
                                frequency[e.InitialChoice]++;
                        }
                    }
                    else
                    {
                        frequency[e.InitialChoice] = 0;
                        if (frequency[e.InitialChoice] < (choice = AvailableDate.GetDateByName(AllLeaves, e.InitialChoice)).availability)
                        {
                            if (e.TryAddChoice(choice))
                                frequency[e.InitialChoice]++;
                        }
                    }
                }
            });
        }

        /*public void BinarySearchSolve() //To use for finding an optimal minimum number of "holes" if the model is infeasible (not yet operational)
        {
            int fstl = Leaves[true].Last().availability, fstr = fstl+Employees.Count,
                sndl = Leaves[false].Last().availability, sndr = sndl+Employees.Count, counter = 0;

            Leaves[false].Last().availability = 1000;

            while (fstl < fstr)
            {
                Console.WriteLine("Try Counter: " + counter.ToString());

                int mid = (fstl + fstr) / 2;

                Leaves[true].Last().availability = mid;
                CpSolverStatus status = Solve();
                if ((status == CpSolverStatus.Infeasible) || (status == CpSolverStatus.Unknown))
                    fstl = mid + 1;
                else
                    fstr = mid;

                counter++;
            }

            Leaves[true].Last().availability = 1000;

            while (sndl < sndr)
            {
                Console.WriteLine("Try Counter: " + counter.ToString());

                int mid = (sndl + sndr) / 2;

                Leaves[false].Last().availability = mid;
                CpSolverStatus status = Solve();
                if ((status == CpSolverStatus.Infeasible) || (status == CpSolverStatus.Unknown))
                    sndl = mid + 1;
                else
                    sndr = mid;

                counter++;
            }

            Leaves[true].Last().availability = fstl;
            Leaves[false].Last().availability = sndl;
            Console.WriteLine(Leaves[true].Last().availability);
            Console.WriteLine(Leaves[false].Last().availability);
        }*/

        public CpSolverStatus Solve()
        {
            CpModel model = new CpModel();

            Dictionary<int, List<IntVar>> Variables = new Dictionary<int, List<IntVar>>();
            List<IntVar> AllVariables = new List<IntVar>();

            Employees.ForEach(e => {
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

            List<IntVar> NilAppears = new List<IntVar>();
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


                if (i != Leaves[true].Count-1 && i != AllLeaves.Count-1)
                    model.AddSumConstraint(ValAppears, 0, AllLeaves.ElementAt(i).availability);
                else
                    NilAppears.AddRange(ValAppears);
            }

            model.Minimize(NilAppears.ToArray().Sum());

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
                AddConflictConstraints(model, e, vars, counter);

                //prev_vars = vars;
            });

            //model.Minimize(AllVariables.ToArray().Sum());

            //Strategy of search
            /*model.AddDecisionStrategy(AllVariables, DecisionStrategyProto.Types.VariableSelectionStrategy.ChooseMinDomainSize,
                                                    DecisionStrategyProto.Types.DomainReductionStrategy.SelectMinValue);*/

            CpSolver solver = new CpSolver
            {
                StringParameters = "max_time_in_seconds:20.0"
            };

            //VarArraySolutionPrinter cb = new VarArraySolutionPrinter(AllVariables.ToArray());
            //solver.SearchAllSolutions(model, cb);
            //solver.SolveWithSolutionCallback(model, cb);
            CpSolverStatus status = solver.Solve(model);

            //Console.WriteLine(String.Format("Optimal objective value: {0}", cb.ObjectiveValue()));
            //Console.WriteLine(String.Format("Number of solutions found: {0}", cb.SolutionCount()));

            if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
            {
                Employees.ForEach(e =>
                {
                    int i = 0;
                    foreach (var val in Variables[e.Id]) //updates Season with the results of the solver
                    {
                        if (i++ == 0 && e.SetDates.Any())
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

                if (!PostSolveConflictCheck())
                    Console.WriteLine("Solution has conflicts!");

                //Console.WriteLine("Model is feasible!");
            }
            else if (status == CpSolverStatus.Infeasible)
                Console.WriteLine("Model is infeasible!");
            else if (status == CpSolverStatus.Unknown)
                Console.WriteLine("Solver timed out!");
            else if (status == CpSolverStatus.ModelInvalid)
                Console.WriteLine("Model Invalid?");

            return status;
        }

        private void AddConflictConstraints(CpModel model, Employee e, IntVar[] vars, int counter) //Conflict Constraints depend on employee type
        {
            bool[,] ConflictArray = null;
            int offset = -1;

            if (e.LeaveType == Employee.LeaveMode.Days5)
                return;
            else if (e.LeaveType == Employee.LeaveMode.Days10)
                return;
            else if (e.LeaveType == Employee.LeaveMode.Days10_5)
            {
                ConflictArray = ConflictAll;
                offset = 0;
            }
            else if (e.LeaveType == Employee.LeaveMode.Days5_5)
            {
                ConflictArray = Conflict5day;
                offset = 0;
            }
            else if (e.LeaveType == Employee.LeaveMode.Days5_5_5)
            {
                ConflictArray = Conflict5day;
                offset = 0;
            }

            if (ConflictArray == null || offset == -1)
                throw new ArgumentException("Mistyped Employee", "e");

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

        public bool PostSolveConflictCheck() //Check if assigned leaves match the required constraints
        {
            foreach (var emp in Employees)
            {
                bool[,] ConflictArray = null;
                int offset = -1;

                if (emp.LeaveType == Employee.LeaveMode.Days5)
                    continue;
                else if (emp.LeaveType == Employee.LeaveMode.Days10)
                    continue;
                else if (emp.LeaveType == Employee.LeaveMode.Days10_5)
                {
                    ConflictArray = ConflictAll;
                    offset = 0;
                }
                else if (emp.LeaveType == Employee.LeaveMode.Days5_5 || emp.LeaveType == Employee.LeaveMode.Days5_5_5)
                {
                    ConflictArray = Conflict5day;
                    offset = 0;
                }

                if (ConflictArray == null || offset == -1)
                    throw new ArgumentException("Mistyped Employee", "e");

                AvailableDate[] arr = emp.SetDates.ToArray();
                for (int i = 0; i < arr.Length; i++)
                    for (int j = i + 1; j < arr.Length; j++)
                        if (ConflictArray[arr[i].Id - offset, arr[j].Id - offset])
                            return false;
            }

            return true;
        }

        private void CalcLUBBasedOnLeaveMode(Employee e, List<AvailableDate> allLeaves, List<int> allLbs, List<int> allUbs)
        {
            e.Lb = new int[e.NumDesiredDates];
            e.Ub = new int[e.NumDesiredDates];

            if (e.LeaveType == Employee.LeaveMode.Days5 || e.LeaveType == Employee.LeaveMode.Days5_5 || e.LeaveType == Employee.LeaveMode.Days5_5_5)
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
                bool isFiveDays = false;
                if (e.SetDates.Any())
                    isFiveDays = (e.SetDates.First().end - e.SetDates.First().start).Days == 4;

                if (!isFiveDays)
                {
                    e.Lb[0] = Leaves[true].Count;
                    e.Ub[0] = allLeaves.Count - 1;
                    allLbs.Add(Leaves[true].Count);
                    allUbs.Add(allLeaves.Count - 1);
                }
                else
                {
                    e.Lb[0] = 0;
                    e.Ub[0] = Leaves[true].Count - 1;
                    allLbs.Add(0);
                    allUbs.Add(Leaves[true].Count - 1);

                    e.Lb[1] = Leaves[true].Count;
                    e.Ub[1] = allLeaves.Count - 1;
                    allLbs.Add(Leaves[true].Count);
                    allUbs.Add(allLeaves.Count - 1);
                }

                for (int i = (isFiveDays ? 2 : 1); i < e.NumDesiredDates; i++)
                {
                    e.Lb[i] = 0;
                    e.Ub[i] = Leaves[true].Count - 1;
                    allLbs.Add(0);
                    allUbs.Add(Leaves[true].Count - 1);
                }
            }
        }

        private void CalculateAndAdjustNeeds() //Add "empty" slots to the model if availability < need. There are two different empty slots, 5day and 10day empty slot.
        {
            int available5 = 0, needed5 = 0, available10 = 0, needed10 = 0;

            Leaves[true].ForEach(l => available5 += l.availability);
            Leaves[false].ForEach(l => available10 += l.availability);

            foreach (var emp in Employees)
            {
                if (emp.LeaveType == Employee.LeaveMode.Days5 || emp.LeaveType == Employee.LeaveMode.Days5_5 || emp.LeaveType == Employee.LeaveMode.Days5_5_5)
                    needed5 += emp.NumDesiredDates;
                else if (emp.LeaveType == Employee.LeaveMode.Days10)
                    needed10 += emp.NumDesiredDates;
                else if (emp.LeaveType == Employee.LeaveMode.Days10_5)
                {
                    needed10 += 1;
                    needed5 += emp.NumDesiredDates - 1;
                }
            }

            if (available5 < needed5)
                Leaves[true].Last().availability = needed5 - available5; //Last element in Leaves[true] is the 5 day empty slot
            if (available10 < needed10)
                Leaves[false].Last().availability = needed10 - available10; //Last element in Leaves[false] is the 10 day empty slot
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