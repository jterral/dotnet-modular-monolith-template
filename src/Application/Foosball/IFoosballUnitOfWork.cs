using Application.Shared;

namespace Application.Foosball;

// Each module has its own DbContext, so a module-specific unit of work avoids two modules fighting over IUnitOfWork.
public interface IFoosballUnitOfWork : IUnitOfWork;
