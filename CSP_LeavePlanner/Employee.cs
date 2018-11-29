using System;
using System.Collections.Generic;
using System.Linq;

class Employee
{
    //Days5 means only 5 day leaves
    //Days10 means only 10 day leaves
    //Days10_5 means one 10day leave and the rest 5 day leaves
    public enum LeaveMode
    {
        Days5,
        Days10,
        Days10_5,
        Days5_5,
        Days5_5_5
    }

    public int Id;
    public List<AvailableDate> SetDates;
    public int NumDesiredDates;
    public String InitialChoice;
    public LeaveMode LeaveType;

    public int[] Lb;
    public int[] Ub;

    public Employee(int id, List<AvailableDate> SetDates, int NumDesiredDates, LeaveMode LeaveType, String choice = null)
    {
        this.Id = id;
        //this.NumDesiredDates = NumDesiredDates;
        this.SetDates = SetDates;
        this.InitialChoice = choice;
        this.LeaveType = LeaveType;
        this.Lb = null;
        this.Ub = null;

        this.NumDesiredDates = NumberOfDatesToAllocate();
    }

    public Employee(Employee e)
    {
        this.Id = e.Id;
        this.SetDates = new List<AvailableDate>();
        e.SetDates.ForEach(s => this.SetDates.Add(new AvailableDate(s)));
        this.NumDesiredDates = e.NumDesiredDates;
        this.InitialChoice = e.InitialChoice;
        this.LeaveType = e.LeaveType;
        this.Lb = new int[e.Lb.Length];
        int c = 0;
        Array.ForEach(e.Lb, i => this.Lb[c++] = i);
        c = 0;
        Array.ForEach(e.Ub, i => this.Ub[c++] = i);
    }

    /*public List<Employee> GetPossibleEmployees(List<AvailableDate> AllDates, TimeSpan ConflictPeriod, out AvailableDate[] datesPicked)
    {
        List<Employee> PossibleEmployees = null;
        List<AvailableDate> datesPickedX = null;
        if (SetDates.Count < NumDesiredDates && AllDates.Any())
        {
            PossibleEmployees = new List<Employee>();
            datesPickedX = new List<AvailableDate>();
            AllDates.ForEach(d => {
                if (!d.Conflicts(SetDates, ConflictPeriod))
                {
                    datesPickedX.Add(d);

                    List<AvailableDate> newSetDates = new List<AvailableDate>();
                    SetDates.ForEach(s => newSetDates.Add(new AvailableDate(s)));

                    Employee emp = new Employee(Id, newSetDates, NumDesiredDates, LeaveMode.Days5, null);
                    emp.AddDate(d);

                    PossibleEmployees.Add(emp);
                }
            });
        }

        datesPicked = (datesPickedX?.ToArray());
        return PossibleEmployees;
    }*/

    public bool AddDate(AvailableDate date) //Adds a date to SetDates
    {
        bool isFiveDays = (date.end - date.start).Days == 4;

        if ((((LeaveType == LeaveMode.Days5 || LeaveType == LeaveMode.Days5_5 || LeaveType == LeaveMode.Days5_5_5) && isFiveDays) ||
            (LeaveType == LeaveMode.Days10 && !isFiveDays) ||
            (LeaveType == LeaveMode.Days10_5))
            && date.availability > 0)
        {
            date.availability--;
            SetDates.Add(date);

            this.NumDesiredDates--;

            return true;
        }

        return false;
    }

    public bool TryAddChoice(Dictionary<bool, List<AvailableDate>> dates)//Adds initial choice of each employee before solving
    {
        AvailableDate date = null;
        if (InitialChoice != null && (LeaveType == LeaveMode.Days5 || LeaveType == LeaveMode.Days5_5 || LeaveType == LeaveMode.Days5_5_5))
            date = AvailableDate.GetDateByName(dates[true], InitialChoice);
        else if (InitialChoice != null && LeaveType == LeaveMode.Days10)
            date = AvailableDate.GetDateByName(dates[false], InitialChoice);
        else if (InitialChoice != null && LeaveType == LeaveMode.Days10_5)
            date = AvailableDate.GetDateByName(dates[false], InitialChoice);
        else
            return true;

        if (date != null && date.availability > 0)
        {
            SetDates.Add(date);

            return true;
        }

        return false;
    }

    public int NumberOfDatesToAllocate()
    {
        int ret = -1;

        if (LeaveType == LeaveMode.Days5)
            ret = 1;
        else if (LeaveType == LeaveMode.Days10)
            ret = 1;
        else if (LeaveType == LeaveMode.Days10_5)
            ret = 2;
        else if (LeaveType == LeaveMode.Days5_5)
            ret = 2;
        else if (LeaveType == LeaveMode.Days5_5_5)
            ret = 3;

        return ret;
    }

    public void Print()
    {
        Console.WriteLine(String.Format("Employee: {0}, NumDesiredDates: {1}, SetDates:", Id, NumDesiredDates));
        SetDates.ForEach(d => d.Print());
    }
}