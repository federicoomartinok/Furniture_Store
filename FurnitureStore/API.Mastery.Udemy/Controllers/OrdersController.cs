using FurnitureStoreData;
using FurnitureStoreModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Mastery.Udemy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly APIcontext _context;

        public OrdersController(APIcontext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<Order>> Get()
        {
            return await _context.Orders.Include(o => o.OrderDetails).ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var order = await _context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Order order)
        {
            if (order == null) return NotFound();

            if (order.OrderDetails == null) 
                return BadRequest("La orden deberia tener al menos 1 detalle");//Devuelve error 400

            await _context.Orders.AddAsync(order);//Aca inserta la orden
            await _context.OrderDetails.AddRangeAsync(order.OrderDetails);//Aca inserta la lista de detalles

            await _context.SaveChangesAsync();

            return CreatedAtAction("Post",order.Id,order);
        }

        [HttpPut]
        public async Task<IActionResult> Put(Order order)
        {
            if (order == null || order.Id <= 0) return NotFound();

            var existingOrder = await _context.Orders.Include(o =>o.OrderDetails).FirstOrDefaultAsync(o => o.Id == order.Id);
            
            if (existingOrder == null) return NotFound();

            existingOrder.OrderNumber = order.OrderNumber;
            existingOrder.OrderDate = order.OrderDate;
            existingOrder.DeliveryDate = order.DeliveryDate;
            existingOrder.ClientId = order.ClientId;

            _context.OrderDetails.RemoveRange(existingOrder.OrderDetails);

            _context.Orders.Update(existingOrder);
            _context.OrderDetails.AddRange(order.OrderDetails);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Order order)
        {
            if (order == null) return NotFound();
            var existingOrder = await _context.Orders.Include(o =>o.OrderDetails).FirstOrDefaultAsync(o =>o.Id == order.Id);
            if (existingOrder == null) return NotFound();

            _context.OrderDetails.RemoveRange(existingOrder.OrderDetails);
            _context.Orders.Remove(existingOrder);

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
