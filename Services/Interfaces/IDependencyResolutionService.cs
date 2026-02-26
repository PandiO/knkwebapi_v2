using System.Threading.Tasks;
using knkwebapi_v2.Dtos;

namespace knkwebapi_v2.Services.Interfaces
{
    /// <summary>
    /// Service for resolving multi-layer dependency field values.
    /// 
    /// Supports navigating through entity relationships to resolve final property values.
    /// Used when a validation rule depends on another field's value, and that value may need
    /// to be transformed by navigating through related entities.
    /// </summary>
    public interface IDependencyResolutionService
    {
        /// <summary>
        /// Batch resolve all dependencies for validation rules on specified fields.
        /// </summary>
        Task<DependencyResolutionResponse> ResolveDependenciesAsync(
            DependencyResolutionRequest request
        );

        /// <summary>
        /// Perform comprehensive health checks on FormConfiguration.
        /// </summary>
        Task<ValidationIssueDto[]> CheckConfigurationHealthAsync(
            int formConfigurationId
        );
    }
}
