// Dafny program filter.dfy compiled into C#
// To recompile, you will need the libraries
//     System.Runtime.Numerics.dll System.Collections.Immutable.dll
// but the 'dotnet' tool in .NET should pick those up automatically.
// Optionally, you may want to include compiler switches like
//     /debug /nowarn:162,164,168,183,219,436,1717,1718

using System;
using System.Numerics;
using System.Collections;
[assembly: DafnyAssembly.DafnySourceAttribute(@"// dafny 4.11.0.0
// Command-line arguments: translate cs --allow-warnings filter.dfy
// filter.dfy


module {:extern ""Bison.Razor.domain_model""} DomainModel {
  class {:extern ""Taxon""} Taxon {
    function {:extern} isSubTaxon(ancestor: Taxon): bool
      decreases ancestor
  }

  class {:extern ""Observation""} Observation {
    function {:extern} getTaxon(): Taxon
  }
}

module filter {
  function FilterBy(root: Taxon, obs: seq<Observation>): seq<Observation>
    decreases root, obs
  {
    if |obs| == 0 then
      []
    else if obs[0].getTaxon() == root || obs[0].getTaxon().isSubTaxon(root) then
      FilterBy(root, obs[1..]) + [obs[0]]
    else
      FilterBy(root, obs[1..])
  }

  import opened DomainModel
}

module filterChecked {
  method FilterByChecked(root: Taxon, obs: seq<Observation>) returns (result: seq<Observation>)
    ensures result == FilterBy(root, obs)
    decreases root, obs
  {
    expect root != null, ""root must not be null"";
    result := FilterBy(root, obs);
  }

  import opened DomainModel

  import opened filter
}
")]

namespace Dafny {
  internal class ArrayHelpers {
    public static T[] InitNewArray1<T>(T z, BigInteger size0) {
      int s0 = (int)size0;
      T[] a = new T[s0];
      for (int i0 = 0; i0 < s0; i0++) {
        a[i0] = z;
      }
      return a;
    }
  }
} // end of namespace Dafny
internal static class FuncExtensions {
  public static Func<UResult> DowncastClone<TResult, UResult>(this Func<TResult> F, Func<TResult, UResult> ResConv) {
    return () => ResConv(F());
  }
  public static Func<U, UResult> DowncastClone<T, TResult, U, UResult>(this Func<T, TResult> F, Func<U, T> ArgConv, Func<TResult, UResult> ResConv) {
    return arg => ResConv(F(ArgConv(arg)));
  }
  public static Func<U1, U2, UResult> DowncastClone<T1, T2, TResult, U1, U2, UResult>(this Func<T1, T2, TResult> F, Func<U1, T1> ArgConv1, Func<U2, T2> ArgConv2, Func<TResult, UResult> ResConv) {
    return (arg1, arg2) => ResConv(F(ArgConv1(arg1), ArgConv2(arg2)));
  }
}
// end of class FuncExtensions
namespace Bison.Razor.domain_model {



} // end of namespace Bison.Razor.domain_model
namespace filter {

  public partial class __default {
    public static Dafny.ISequence<Bison.Razor.domain_model.Observation> FilterBy(Bison.Razor.domain_model.Taxon root, Dafny.ISequence<Bison.Razor.domain_model.Observation> obs)
    {
      Dafny.ISequence<Bison.Razor.domain_model.Observation> _0___accumulator = Dafny.Sequence<Bison.Razor.domain_model.Observation>.FromElements();
    TAIL_CALL_START: ;
      if ((new BigInteger((obs).Count)).Sign == 0) {
        return Dafny.Sequence<Bison.Razor.domain_model.Observation>.Concat(Dafny.Sequence<Bison.Razor.domain_model.Observation>.FromElements(), _0___accumulator);
      } else if (((((obs).Select(BigInteger.Zero)).getTaxon()) == (object) (root)) || ((((obs).Select(BigInteger.Zero)).getTaxon()).isSubTaxon(root))) {
        _0___accumulator = Dafny.Sequence<Bison.Razor.domain_model.Observation>.Concat(Dafny.Sequence<Bison.Razor.domain_model.Observation>.FromElements((obs).Select(BigInteger.Zero)), _0___accumulator);
        Bison.Razor.domain_model.Taxon _in0 = root;
        Dafny.ISequence<Bison.Razor.domain_model.Observation> _in1 = (obs).Drop(BigInteger.One);
        root = _in0;
        obs = _in1;
        goto TAIL_CALL_START;
      } else {
        Bison.Razor.domain_model.Taxon _in2 = root;
        Dafny.ISequence<Bison.Razor.domain_model.Observation> _in3 = (obs).Drop(BigInteger.One);
        root = _in2;
        obs = _in3;
        goto TAIL_CALL_START;
      }
    }
  }
} // end of namespace filter
namespace filterChecked {

  public partial class __default {
    public static Dafny.ISequence<Bison.Razor.domain_model.Observation> FilterByChecked(Bison.Razor.domain_model.Taxon root, Dafny.ISequence<Bison.Razor.domain_model.Observation> obs)
    {
      Dafny.ISequence<Bison.Razor.domain_model.Observation> result = Dafny.Sequence<Bison.Razor.domain_model.Observation>.Empty;
      if (!((root) != (object) ((Bison.Razor.domain_model.Taxon)null))) {
        throw new Dafny.HaltException("filter.dfy(31,4): " + Dafny.Sequence<Dafny.Rune>.UnicodeFromString("root must not be null").ToVerbatimString(false));}
      result = filter.__default.FilterBy(root, obs);
      return result;
    }
  }
} // end of namespace filterChecked
namespace _module {

} // end of namespace _module
