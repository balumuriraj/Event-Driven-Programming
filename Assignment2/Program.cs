using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Assignment2.encryptdecrypt;

namespace Assignment2
{
    public delegate void pricecutdelegate(int price); // delegate declaration for price cut
    public delegate void processcomplete(OrderClass order); // delegate declaration for order completion

    class ChickenFarm
    {
        int currentprice = 90;
        int pricechange = 0;

        public static event pricecutdelegate pricecutevent; // Define an event for price cut

        public void EventEmitter()
        {
            if (pricecutevent != null)
            {
                pricecutevent(currentprice); // emit an event for price cut
            } 
        }

        public int getcurrentprice() // get current price
        {
            return currentprice; 
        }

        public void PricingModel()
        {
            while(true)
            {
                Random r = new Random(); 
                int temp = r.Next(80, 100); // Generate new random chicken price
                if (temp < currentprice) // Chicken price reduced
                {
                    currentprice = temp;
                    pricechange++;
                    System.Console.WriteLine("Chicken price reduced to: " + currentprice);
                    System.Console.WriteLine();
                    System.Console.WriteLine("=========================Price Cut Event============================");
                    System.Console.WriteLine();
                    EventEmitter();
                    
                }
                else // Chicken price raised
                {
                    currentprice = temp;
                    System.Console.WriteLine("Chicken price raised to: " + currentprice);
                }

                if (pricechange == 10) //Abort when price reduced for 10 times.
                {
                    Thread.CurrentThread.Abort();
                }

                Thread.Sleep(500);
            }
        }

        public OrderClass Decoder(String str)
        {
            encryptdecrypt.Service proxy = new encryptdecrypt.Service(); //Using web service to decrypt
            String decrypted = proxy.Decrypt(str); // Decrypt the given order string

            //convert string to object
            OrderClass o = new OrderClass();
            String[] split = decrypted.Split('-');
            o.senderID = Convert.ToInt32(split[0]);
            o.cardNo = Convert.ToInt32(split[1]);
            o.amount = Convert.ToInt32(split[2]);
            o.timesent = Convert.ToDateTime(split[3]);

            return o;
        }

        public void recieveOrder()
        {
            while (true)
            {
                while (MultiCellBuffer.index >= 0) //enter while there exists an object in the buffer
                {
                    String str = MultiCellBuffer.getOneCell(); // grab a string order from buffer
                    OrderClass order = new OrderClass();
                    order = Decoder(str); // Decode the string order
                    OrderProcessing op = new OrderProcessing(order, currentprice);
                    Thread t = new Thread(new ThreadStart(op.processorder));
                    t.Start(); //start thread for each order processing
                }
            }

        }

    }

    class OrderProcessing
    {
        OrderClass order;
        int price;
        int tax;
        int totalprice;
        int shipping = 10;

        // Define events for all the retailers
        public static event processcomplete ordercompleteevent1;
        public static event processcomplete ordercompleteevent2;
        public static event processcomplete ordercompleteevent3;
        public static event processcomplete ordercompleteevent4;
        public static event processcomplete ordercompleteevent5;

        public void EventEmitter()
        {
            if (order.senderID == 1)
            {
                if (ordercompleteevent1 != null)
                {
                    ordercompleteevent1(order); // emit an event for retailer 1
                } 
            }
            else if (order.senderID == 2)
            {
                if (ordercompleteevent2 != null)
                {
                    ordercompleteevent2(order); // emit an event retailer 2
                } 
            }
            else if (order.senderID == 3)
            {
                if (ordercompleteevent3 != null)
                {
                    ordercompleteevent3(order); // emit an event retailer 3
                } 
            }
            else if (order.senderID == 4)
            {
                if (ordercompleteevent4 != null)
                {
                    ordercompleteevent4(order); // emit an event retailer 4
                } 
            }
            else if (order.senderID == 5)
            {
                if (ordercompleteevent5 != null)
                {
                    ordercompleteevent5(order); // emit an event retailer 5
                } 
            }
            
        }

        public OrderProcessing(OrderClass o, int p) // constructor for initializing the order and price
        {
            order = o;
            price = p;
        }

        public void processorder()
        {
            if (5000 <= order.cardNo && order.cardNo <= 7000) // Valid card No condition
            {
                System.Console.WriteLine();
                System.Console.WriteLine("The card No " + order.cardNo + " of Retailer " + order.senderID + " is valid!");
                tax = ((price * order.amount) * 8) / 100; // calcute tax
                totalprice = (price * order.amount) + tax + shipping; // calculate total price
                System.Console.WriteLine("The order for Retailer " + order.senderID + " is processed. Here is the confirmation: ");
                System.Console.WriteLine();
                System.Console.WriteLine("--------------------------CONFIRMATION----------------------------");
                System.Console.WriteLine("The Number of Chickens ordered: " + order.amount);
                System.Console.WriteLine("Price of each unit: " + price);
                System.Console.WriteLine("The Total price without tax and shipping: " + (price * order.amount));
                System.Console.WriteLine("Tax: " + tax);
                System.Console.WriteLine("Shipping: " + shipping);
                System.Console.WriteLine("Total Price: " + totalprice);
                System.Console.WriteLine("-------------------------------------------------------------------");
                EventEmitter(); // Emit Event for order completion
            }
            else
            {
                System.Console.WriteLine("The card No of Retailer " + order.senderID + " is invalid!");
            }
        }

    }

    class Retailer
    {
        OrderClass o = new OrderClass();

        public Retailer(int senderID, int cardNo) // constructor for initializing the retailer
        {
            o.senderID = senderID;
            o.cardNo = cardNo;
        }

        public void PriceCutEventHandler(int price)
        {
            Random r = new Random();
            o.amount = r.Next(10, 20); // Generate random amount of chicken to order
            o.timesent = DateTime.Now; // save the time of generating the order
            String encrypted = Encoder(o); // Encode the object to string

            System.Console.WriteLine(o.timesent + ": Sending Retailer " + o.senderID + "'s order of " + o.amount + " Chicken...");
            MultiCellBuffer.setOneCell(o, encrypted); // send string order to buffer
        }

        public void OrderCompleteEventHandler(OrderClass order) ///retailer receives order completion confirmation and prints the time completion for the order
        {
            DateTime timerec = DateTime.Now;
            System.Console.WriteLine("Retailer " + order.senderID + " received confirmation of order. Time to complete: " + (timerec - o.timesent));
            System.Console.WriteLine();
        }

        public String Encoder(OrderClass order)
        {
            String objstring = o.senderID.ToString() + "-" + o.cardNo.ToString() + "-" + o.amount.ToString() + "-" + o.timesent.ToString();

            encryptdecrypt.Service proxy = new encryptdecrypt.Service(); //Using web service to encrypt
            String encrypted = proxy.Encrypt(objstring); // Encryption

            return encrypted;
        }


    }

    public class OrderClass
    {
        public int senderID { get; set; }
        public int cardNo { get; set; }
        public int amount { get; set; }
        public DateTime timesent { get; set; }
    }

    public static class MultiCellBuffer
    {
        public static String[] datacells = new String[3]; // buffer 
        public static int index = -1; // current position in the buffer
        public static Semaphore sem;

        public static void setOneCell(OrderClass o, String order)
        {
            sem.WaitOne(); 
            Monitor.Enter(datacells); //enter monitor
            try
            {
                if (index < 2)
                {
                    index++;
                    datacells[index] = order; //insert order into buffer
                    System.Console.WriteLine("Retailer " + o.senderID + "'s order sent!");
                    
                }
            }

            finally
            {
                Monitor.Exit(datacells); // exit monitor
            }
            sem.Release(); //Semaphore Released
        }

        public static String getOneCell()
        {
            String order = null;
            sem.WaitOne();
            Monitor.Enter(datacells);
            try
            {
                if (index >= 0)
                {
                    order = datacells[index]; // retrieve order from buffer
                    index--;
                }
            }

            finally
            {
                Monitor.Exit(datacells); // exit monitor
            }
            sem.Release(); //Semaphore Released
            return order;
            
        }


        class Program
        {
            static void Main(string[] args)
            {
                System.Console.WriteLine("**********************************Assignment 2*********************************");
                System.Console.WriteLine();

                sem = new Semaphore(0, 3);
                sem.Release(3);

                ChickenFarm chickenfarm = new ChickenFarm();
                Thread pricemodelthread = new Thread(new ThreadStart(chickenfarm.PricingModel));
                pricemodelthread.Start(); // Starts the chicken farm thread that does price change

                // Create Retailer objects and add event methods
                Retailer retailer1 = new Retailer(1, 5000);
                ChickenFarm.pricecutevent += new pricecutdelegate(retailer1.PriceCutEventHandler);

                Retailer retailer2 = new Retailer(2, 5500);
                ChickenFarm.pricecutevent += new pricecutdelegate(retailer2.PriceCutEventHandler);

                Retailer retailer3 = new Retailer(3, 6000);
                ChickenFarm.pricecutevent += new pricecutdelegate(retailer3.PriceCutEventHandler);

                Retailer retailer4 = new Retailer(4, 6500);
                ChickenFarm.pricecutevent += new pricecutdelegate(retailer4.PriceCutEventHandler);

                Retailer retailer5 = new Retailer(5, 7000);
                ChickenFarm.pricecutevent += new pricecutdelegate(retailer5.PriceCutEventHandler);

                // Create Retailer threads
                Thread[] retailers = new Thread[5];

                Thread orderprocess = new Thread(new ThreadStart(chickenfarm.recieveOrder));
                orderprocess.Start(); // Start order processing thread that starts threads for each process.

                //add event methods for sending order completion conformation
                OrderProcessing.ordercompleteevent1 += new processcomplete(retailer1.OrderCompleteEventHandler);
                OrderProcessing.ordercompleteevent2 += new processcomplete(retailer2.OrderCompleteEventHandler);
                OrderProcessing.ordercompleteevent3 += new processcomplete(retailer3.OrderCompleteEventHandler);
                OrderProcessing.ordercompleteevent4 += new processcomplete(retailer4.OrderCompleteEventHandler);
                OrderProcessing.ordercompleteevent5 += new processcomplete(retailer5.OrderCompleteEventHandler);

                System.Console.ReadLine();
            }
        }
    }
}