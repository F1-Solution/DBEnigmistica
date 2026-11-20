namespace DBEnigmistica.Tests.Fixtures;

/// <summary>Le prove che cambiano le variabili d'ambiente del processo non girano in parallelo con le altre.</summary>
[CollectionDefinition("AmbienteProcesso", DisableParallelization = true)]
public sealed class AmbienteProcessoCollection;
