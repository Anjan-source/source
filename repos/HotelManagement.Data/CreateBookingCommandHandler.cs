using System;
using System.Collections.Generic;
using System.Text;

namespace BookingManagement.Data
{
    class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, CustomQueryResult<OutputObject>>
    {
        public async Task<CustomQueryResult<ProjectManagementItemSlim>> Handle(
          CreateBookingCommand request,
          CancellationToken cancellationToken)
        {
            //Build SQL query 

            //Using dapper send aysnc calls to database

            return await ExecuteInManagedConnectionAsync(async connection =>
            {
                long totalResults;

                IEnumerable<BookingDetails> items;

                
                    items = await connection
                        .QueryAsync<BookingDetails>(
                            sql,
                            parameters,
                            commandType: CommandType.Text
                        ).ConfigureAwait(false);

                    items = items.Distinct().ToList();
                

                totalResults = items.FirstOrDefault()?.TotalResults ?? 0;

                
                return new CustomQueryResult<OutputType>(items, totalResults);
            }).ConfigureAwait(false);
        }
    }
}
