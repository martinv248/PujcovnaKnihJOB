using FluentEmail.Core;
using FluentEmail.Razor;
using FluentEmail.Smtp;
using PujcovnaKnihJOB.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
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
                    //Console.WriteLine("Titul: " + book.Title + " Status: " + book.IsAvailable + " Order state: " + order.State);
                    book.IsAvailable = "Půjčena";
                    order.State = "Půjčena";
                    order.BorrowDate = DateTime.Now;
                    //Console.WriteLine("Titul: " + book.Title + " Status: " + book.IsAvailable + " Order state: " + order.State + " Date: " + order.BorrowDate);
                    Console.WriteLine("Kniha " + book.Title + " byla zapůjčena");
                    db.Entry(book).State = EntityState.Modified;
                    db.Entry(order).State = EntityState.Modified;    
                } else if(order.State.Equals(returnState))
                {
                    Books book = db.Books.Find(order.BookID);
                    //Console.WriteLine("Titul: " + book.Title + " Status: " + book.IsAvailable + " Order state: " + order.State);
                    book.IsAvailable = "Volná";
                    order.State = "Vrácena";
                    order.ReturnDate = DateTime.Now;
                    //Console.WriteLine("Titul: " + book.Title + " Status: " + book.IsAvailable + " Order state: " + order.State + " Date: " + order.ReturnDate);
                    Console.WriteLine("Kniha " + book.Title + "  byla vrácena");
                    db.Entry(book).State = EntityState.Modified;
                    db.Entry(order).State = EntityState.Modified;
                }
            }
            db.SaveChanges();
        }

        public async Task invoiceRents()
        {
            var sender = new SmtpSender(() => new SmtpClient("localhost")
            {
                EnableSsl = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Port = 25
            });

            StringBuilder template = new StringBuilder();
            template.AppendLine("Vážený zákazníku,");
            template.AppendLine("<p>Děkujeme za to, že jste si od nás zapůjčili knihu @Model.BookTitle.</p>");
            template.AppendLine("<p>Údaje k fakturaci jsou uvedené níže.</p>");
            template.AppendLine("<p><b>Jméno a příjmení:</b> @Model.FName @Model.LName</p>");
            template.AppendLine("<p><b>Číslo objednávky:</b> @Model.OrderID</p>");
            template.AppendLine("<p><b>Cena za den:</b> @Model.Price Kč</p>");
            template.AppendLine("<p><b>Den zapůjčení:</b> @Model.BorrowDate</p>");
            template.AppendLine("<p><b>Den vrácení:</b> @Model.ReturnDate</p>");
            template.AppendLine("<p><b>Celková cena:</b> @Model.FullPrice Kč</p>");
            template.AppendLine("<p>S pozdravem</p>");
            template.AppendLine("<p>Vaše Půjčovna knih</p>");

            var orders = db.Orders.Where(o => o.State.Equals("Vrácena") && o.Invoiced.Equals("Ne"));
            foreach(var order in orders)
            {
                DateTime borrowDay = Convert.ToDateTime(order.BorrowDate);
                var bDay = borrowDay.Date;
                DateTime returnDay = Convert.ToDateTime(order.ReturnDate);
                var rDay = returnDay.Date;
                var time = ((TimeSpan)(rDay - bDay)).Days;
                decimal finalPrice;
                
                var user = db.Users.Find(order.CustomerID);
                var book = db.Books.Find(order.BookID);

                if (time == 0)
                {
                    finalPrice = book.Price;
                } else
                {
                    finalPrice = (time + 1) * book.Price;
                }

                Email.DefaultSender = sender;
                Email.DefaultRenderer = new RazorRenderer();

                var email = await Email
                    .From("admin@pujcovnaknih.com")
                    .To(user.Email)
                    .Subject("Děkujeme, že jste si půjčili naši knihu!")
                    .UsingTemplate(template.ToString(), new { FName = user.FName, LName = user.LName, BookTitle = book.Title, OrderID = order.ID, Price = book.Price, 
                        BorrowDate = order.BorrowDate, ReturnDate = order.ReturnDate, FullPrice = finalPrice })
                    .SendAsync();
                order.Invoiced = "Ano";
                order.InvoiceDate = DateTime.Now;
                db.Entry(order).State = EntityState.Modified;
            }
            db.SaveChanges();
        }
    }
}
