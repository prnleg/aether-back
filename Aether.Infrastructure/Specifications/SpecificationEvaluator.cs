using Aether.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Aether.Infrastructure.Specifications;

public static class SpecificationEvaluator<T> where T : class
{
    public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> specification)
    {
        var query = inputQuery.Where(specification.Criteria);

        query = specification.Includes.Aggregate(query,
            (current, include) => current.Include(include));

        return query;
    }
}
