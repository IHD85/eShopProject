using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Catalog.API.UnitTests
{
    public static class MockDbSetExtensions
    {
        public static Mock<DbSet<T>> AsMockDbSet<T>(this IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();

            // Simulerer synkrone/asynkrone LINQ-operationer
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());

            // Simulerer asynkrone operationer (hvis du ville bruge FirstOrDefaultAsync, ToListAsync osv.)
            // Da du bruger ToList(), er det ikke strengt nødvendigt for den metode, men godt at have:
            // mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(default))
            //     .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));

            return mockSet;
        }
    }
}
