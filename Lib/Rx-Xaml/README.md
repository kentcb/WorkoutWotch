# ReactiveUI

Without a reference to *System.Reactive.Windows.Threading.dll*, ReactiveUI cannot bootstrap when running our unit tests under .NET 4.5. The reason it's in *Lib* instead of *packages* is that none of our projects target .NET runtime (otherwise I could just add a package dependency there).