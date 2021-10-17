{
  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs";
    neo4j-d.url = "/home/artem/nixos_main/desktop/neo4j-desktop/default.nix";
    neo4j-d.flake = false;
  };

  outputs = { self, nixpkgs, neo4j-d, ... }:
    let
      pkgs = import nixpkgs { system = "x86_64-linux";
      config.allowUnfreePredicate = pkg: builtins.elem (nixpkgs.lib.getName pkg) [
             "neo4j-desktop"
           ];
      };
    in
      {

        devShell.x86_64-linux = pkgs.mkShell {

          buildInputs = with pkgs; [
            docker
            dotnet-sdk_3
          ];
          shellHook = ''
          '';
        };
      };
}
