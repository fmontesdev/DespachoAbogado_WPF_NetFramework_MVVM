/// <summary>
/// Configuración global de MSTest para el proyecto de pruebas.
/// Habilita la paralelización a nivel de método para mejorar el rendimiento de las pruebas
/// </summary>
[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]
