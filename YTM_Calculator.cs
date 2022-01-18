using System;
using System.Collections.Generic;

//Author: Lewis Broderick-Gatrell

namespace YTM_Calculator
{
    struct Cashflow   /*defining the object for one single cashflow which contains a date, principle and coupon. Struct is used instead 
                        of a class because it defines an immutable value-type object rather than a reference type object */
    {
        public DateTime paymentDate;
        public double PrincipleAmount;
        public double CouponAmount;
        
        public Cashflow(DateTime date, double principle, double coupon) //constructor used to assign fields for this object
        {
            paymentDate = date;
            PrincipleAmount = principle;
            CouponAmount = coupon;

        }
    }

    public class Cashflows      //defines the object for multiple cashflows, which is a list of the singular cashflow object defined above
    {
        List<Cashflow> Flows;

        public Cashflows()   //default constructor 
        {
            Flows = new List<Cashflow>();
        }

        public void Add_Flow(DateTime date, double principle, double coupon) //this function adds a defined cashflow object to the cashflows list
        {
            Cashflow cashflow = new Cashflow(date, principle, coupon);
            Flows.Add(cashflow);

        }

        public double Total_Principle() //calculates the total principle from a bonds cashflow. Necessary for the scenario in which the principle of the bond is not fully paid at maturity
        {
            double TotalPrincipleAmt = 0;
            foreach (Cashflow c in Flows)
            {
                TotalPrincipleAmt += c.PrincipleAmount;
            }

            return TotalPrincipleAmt;
        }

        public double Bond_Price(DateTime PricingDate, double r)  //calculates a bonds price at a given yield and date using the cashflows within this class. Returns the value as a percentage of par
        {
            double price = 0;
            double par = Total_Principle();
            foreach (Cashflow c in Flows)
            {
                DateTime enddate = c.paymentDate;
                double years = (enddate.Date - PricingDate.Date).Days / 365;   //assuming 365 days a year each year in order to get years until this coupon/principle payment
                price += (c.PrincipleAmount + c.CouponAmount) / Math.Pow(1 + r, years);
            }
            return price/par * 100;
        }


        private double Function(double r, DateTime PricingDate, double PresentValuePar)  /*calculates difference between actual bond price and the implied bond price from cashflows and yield. 
                                                                                        this function is used in the Newton-Raphson algorithm in order to find the root which is the YTM */
        {
            double total_principle = Total_Principle();
            double p = PresentValuePar * total_principle;           

            foreach (Cashflow c in Flows)
            { DateTime enddate = c.paymentDate;
                double years = (enddate.Date - PricingDate.Date).Days/365.0;   //assuming 365 days a year each year in order to get the number of years until this coupon/principle payment
                p -= (c.PrincipleAmount + c.CouponAmount) / Math.Pow(1+r, years);                                                            

            }

            return p;
        }

        private double Function_Derivative(double r, DateTime PricingDate)  //derivative of the function defined above. Needed for the Newton-Raphson algorithm
        {
            double s = 0;           /*set initial price of bond to be 0 the calculator then iterates through 
                                     each cashflow */
            foreach (Cashflow c in Flows)
            {
                DateTime enddate = c.paymentDate;
                double years = (enddate.Date - PricingDate.Date).Days / 365.0;   //assuming 365 days a year in order to get years until this coupon/principle payment
                s += years*(c.PrincipleAmount + c.CouponAmount) / Math.Pow(1 + r, 1+years);

            }

            return s;
        }

        public double[] YTM_Calculator(DateTime PricingDate, double PresentValuePar )  //Uses the Newton-Raphson Method to find the YTM
        {
            double r = 0.04; //Choose 4% YTM as our initial value 
            double iterations = 0;

            while(Math.Abs(Function(r, PricingDate, PresentValuePar)) > 0.001) //error value based on trial and error
            {
                r = r - Function(r, PricingDate, PresentValuePar) / Function_Derivative(r, PricingDate); //iterates this process until the difference between the actual and calcualted pv is greater than specified value
                iterations++;
            }

            double[] output = { r, iterations };  //outputs an array containing the YTM and the number of iterations the Newton-Raphson took in order to monitor its efficiency
            return output;

        }

    }

    class Program
    {

        static void Main(string[] args)
        {
            //Here are some examples to demonstrate the calculator
            Cashflows cashflows = new Cashflows();
            cashflows.Add_Flow(new DateTime(2024, 05, 20), 0, 22750); 
            cashflows.Add_Flow(new DateTime(2024, 11, 20), 0, 22750);  
            cashflows.Add_Flow(new DateTime(2025, 05, 20), 0, 22750);
            cashflows.Add_Flow(new DateTime(2025, 11, 20), 0, 22750);
            cashflows.Add_Flow(new DateTime(2026, 05, 20), 0, 22750);
            cashflows.Add_Flow(new DateTime(2026, 11, 20), 0, 22750);
            cashflows.Add_Flow(new DateTime(2027, 05, 20), 0, 22750);
            cashflows.Add_Flow(new DateTime(2027, 11, 20), 0, 22750);
            cashflows.Add_Flow(new DateTime(2028, 05, 20), 1000000, 22750);

            //YTM for a bond priced at 102% of par as of 27/03/2021
            double ParValue1 = 1.02;
            double[] rA = cashflows.YTM_Calculator(new DateTime(2021, 03, 27), ParValue1);
            Console.WriteLine("The YTM was calculated to be " + rA[0]*100 + "%.\nThis took the Newton-Raphson algorithm " + rA[1] +" iterations to compute.");
            Console.WriteLine("\n");

            //YTM for a bond priced at 98% of par as of 27/03/2021
            double ParValue2 = 0.98;
            double[] rB = cashflows.YTM_Calculator(new DateTime(2021, 03, 27), ParValue2);
            Console.WriteLine("The YTM was calculated to be " + rB[0] * 100 + "%.\nThis took the Newton-Raphson algorithm " + rB[1] + " iterations to compute.");
            Console.WriteLine("\n");

            //YTM for a bond priced 102% of par as of 07/04/2021
            double[] rC = cashflows.YTM_Calculator(new DateTime(2021, 04, 07), ParValue1);
            Console.WriteLine("The YTM was calculated to be " + rC[0] * 100 + "%.\nThis took the Newton-Raphson algorithm " + rC[1] + " iterations to compute.");
            Console.WriteLine("\n");

            //Calculating the bond price at 4.22% yield as of 27/03/2021
            double bp = cashflows.Bond_Price(new DateTime(2021, 03, 27), 0.0422);
            Console.WriteLine("The Bond Price is " + bp + "% of par");
            Console.WriteLine("\n");

            //Additional Cashflows are added and the calculations above are repeated
            Cashflows cashflow2 = cashflows;
            cashflow2.Add_Flow(new DateTime(2021, 05, 20), 0, 22750);
            cashflow2.Add_Flow(new DateTime(2021, 11, 20), 0, 22750);
            cashflow2.Add_Flow(new DateTime(2022, 05, 20), 0, 22750);
            cashflow2.Add_Flow(new DateTime(2022, 11, 20), 0, 22750);
            cashflow2.Add_Flow(new DateTime(2023, 05, 20), 0, 22750);
            cashflow2.Add_Flow(new DateTime(2023, 11, 20), 0, 22750);


            //YTM for a bond priced 102% of par as of 27/03/2021
            double[] r1 = cashflow2.YTM_Calculator(new DateTime(2021, 03, 27), ParValue1);
            Console.WriteLine("The YTM was calculated to be " + r1[0] * 100 + "%.\nThis took the Newton-Raphson algorithm " + r1[1] + " iterations to compute.");
            Console.WriteLine("\n");

            //YTM for a bond priced at 98% of par as of 27/03/2021
            double[] r2 = cashflow2.YTM_Calculator(new DateTime(2021, 03, 27), ParValue2);
            Console.WriteLine("The YTM was calculated to be " + r2[0] * 100 + "%.\nThis took the Newton-Raphson algorithm " + r2[1] + " iterations to compute.");
            Console.WriteLine("\n");

            //YTM for a bond priced at 102% of par as of 07/04/2021
            double[] r3 = cashflow2.YTM_Calculator(new DateTime(2021, 04, 07), ParValue1);
            Console.WriteLine("The YTM was calculated to be " + r3[0] * 100 + "%.\nThis took the Newton-Raphson algorithm " + r3[1] + " iterations to compute.");
            Console.WriteLine("\n");

            //Calculating the bond price at 4.22% yield as of 27/03/2021
            double bpE = cashflow2.Bond_Price(new DateTime(2021, 03, 27), 0.0422);
            Console.WriteLine("The Bond Price is " + bpE +"% of par");

        }
    }
}
