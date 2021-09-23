using PujcovnaKnihJOB.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PujcovnaKnihJOB.Services
{
    class ModelServices
    {
        private dbEntities db = new dbEntities();

        public void checkBooks()
        {
            var orders = from m in db.Orders select m;
            var books = from n in db.Books select n;
            string borrowState = "Žádost o zapůjčení";
            string returnState = "Žádost o vrácení";

            foreach(var order in orders)
            {
                if (order.State.Equals(borrowState))
                {
                    Books book = db.Books.Find(order.BookID);
                    Console.WriteLine("Titul: " + book.Title + " Status: " + book.IsAvailable + " Order state: " + order.State);
                    book.IsAvailable = "Půjčena";
                    order.State = "Půjčena";
                    order.BorrowDate = DateTime.Now;
                    Console.WriteLine("Titul: " + book.Title + " Status: " + book.IsAvailable + " Order state: " + order.State + " Date: " + order.BorrowDate);
                    db.Entry(book).State = EntityState.Modified;
                    db.Entry(order).State = EntityState.Modified;    
                } else if(order.State.Equals(returnState))
                {
                    Books book = db.Books.Find(order.BookID);
                    Console.WriteLine("Titul: " + book.Title + " Status: " + book.IsAvailable + " Order state: " + order.State);
                    book.IsAvailable = "Volná";
                    order.State = "Vrácena";
                    order.ReturnDate = DateTime.Now;
                    Console.WriteLine("Titul: " + book.Title + " Status: " + book.IsAvailable + " Order state: " + order.State + " Date: " + order.ReturnDate);
                    db.Entry(book).State = EntityState.Modified;
                    db.Entry(order).State = EntityState.Modified;
                }
            }
            db.SaveChanges();
        }
    }
}
