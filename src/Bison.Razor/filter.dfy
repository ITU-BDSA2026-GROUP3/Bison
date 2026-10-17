module {:extern "Bison.Razor.domain_model"} DomainModel {
  class {:extern "Taxon"} Taxon {
    function {:extern} isSubTaxon(ancestor: Taxon): bool
  }

  class {:extern "Observation"} Observation {
    function {:extern} getTaxon(): Taxon
  }
}

module filter {
  import opened DomainModel

  function FilterBy(root: Taxon, obs: seq<Observation>): seq<Observation>
  {
    if |obs| == 0 then []
    else if obs[0].getTaxon() == root ||
            obs[0].getTaxon().isSubTaxon(root)
    then FilterBy(root, obs[1..]) + [obs[0]]
    else FilterBy(root, obs[1..])
  }
}

module filterChecked {
  import opened DomainModel
  import opened filter

  method FilterByChecked(root: Taxon, obs: seq<Observation>) returns (result: seq<Observation>)
    ensures result == FilterBy(root, obs)
  {
    expect root != null, "root must not be null";
    result := FilterBy(root, obs);
  }
}