using SharpDockerizer.AppLayer.Models;

namespace SharpDockerizer.AppLayer.Contracts;
public interface IDockerfileGenerator
{
    string Execute(DockerfileGeneratorInputModel model);
}
