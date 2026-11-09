export interface ModelInfo {
  name: string;
  size: number;
}

export type ParamKind = "int" | "float" | "str" | "bool" | "select";

export interface ParamDef {
  flag: string;
  short: string | null;
  name: string;
  group: string;
  kind: ParamKind;
  options: string[];
  default: string | null;
  help: string;
}

export interface ParamEntry {
  flag: string;
  value: string | null;
  enabled: boolean;
}

export interface Profile {
  name: string;
  model: string;
  port: number;
  image: string | null;
  params: ParamEntry[];
  rawArgs: string[];
}

export interface ContainerStatus {
  profile: string;
  running: boolean;
  state: string;
  port: number;
  model: string;
}

export interface HfFile {
  path: string;
  size: number;
}

export interface HfProgress {
  received: number;
  total: number;
  speed: number;
  done: boolean;
  error: string | null;
}

export interface BuildArgs {
  cudaVersion: string;
  ubuntuVersion: string;
  llamaCppRef: string;
  cudaArch: string;
}

export interface AppConfig {
  modelsPath: string;
  dockerfile: string;
  image: string;
  portBase: number;
  hfToken: string | null;
  buildArgs: BuildArgs;
}
