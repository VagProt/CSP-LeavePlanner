using System;
using System.Collections.Generic;

class AvailableDate
{
    public int Id;
    public String name;
    public DateTime start;
    public DateTime end;
    public int availability;

    public AvailableDate(int Id, String name, DateTime start, DateTime end, int availability)
    {
        this.Id = Id;
        this.name = name;
        this.start = start;
        this.end = end;
        this.availability = availability;
    }

    public AvailableDate(AvailableDate date)
    {
        this.Id = date.Id;
        this.name = new String(date.name.ToCharArray());
        this.start = date.start;
        this.end = date.end;
        this.availability = date.availability;
    }

    public override bool Equals(Object obj)
    {
        //Check for null and compare run-time types.
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            AvailableDate p = (AvailableDate)obj;
            return (Id == p.Id) && (name == p.name);
        }
    }

    public bool Conflicts(List<AvailableDate> dates, TimeSpan ConflictPeriod) //multiple check (not used)
    {
        bool flag = false;
        AvailableDate[] DateArr = dates.ToArray();
        for (int i = 0; i < DateArr.Length; i++)
        {
            TimeSpan diff1 = this.start - DateArr[i].end;
            TimeSpan diff2 = DateArr[i].start - this.end;
            if (diff1.Days > 0 && diff1.Days < ConflictPeriod.Days)
            {
                flag = true;
                break;
            }
            else if (diff2.Days > 0 && diff2.Days < ConflictPeriod.Days)
            {
                flag = true;
                break;
            }
            else if (diff1.Days < 0 && diff2.Days < 0)
            {
                flag = true;
                break;
            }
        }

        return flag;
    }

    public bool Conflicts(AvailableDate date, TimeSpan ConflictPeriod) //Conflict between this and date
    {
        bool flag = false;

        if (date.name == "FAKE" || this.name == "FAKE")
            return flag;

        TimeSpan diff1 = this.start - date.end;
        TimeSpan diff2 = date.start - this.end;
        if (diff1.Days > 0 && diff1.Days < ConflictPeriod.Days)
            flag = true;
        else if (diff2.Days > 0 && diff2.Days < ConflictPeriod.Days)
            flag = true;
        else if (diff1.Days < 0 && diff2.Days < 0)
            flag = true;

        return flag;
    }

    public void Print()
    {
        Console.WriteLine(String.Format("AvailableDate: {4}, {0}, Duration: {1} -> {2}, Availability: {3}", name, start.Day, end.Day, availability, Id));
    }

    public static bool[,] CreateRelationTable(List<AvailableDate> dates, TimeSpan conflictPeriod) //Creates the conflict table between pairs of dates
    {
        AvailableDate[] datesArr = dates.ToArray();
        bool[,] ConflictArray = new bool[datesArr.Length, datesArr.Length];

        for (int i = 0; i < datesArr.Length; i++)
            for (int j = 0; j < datesArr.Length; j++)
                ConflictArray[i, j] = datesArr[i].Conflicts(datesArr[j], conflictPeriod);

        return ConflictArray;
    }

    public static AvailableDate GetDateByName(List<AvailableDate> dates, String name)
    {
        return dates.Find(i => i.name == name);
    }
}