# Renders the placeholder music: short original tunes, played by old-console voices (two pulses, a triangle, soft noise)
# into 16-bit PCM mono 22 050 Hz WAVs, the format the game plays. The game can't play MIDI, so the notes live here.
# Placeholders only, until the developer's real music.
#
#   perl .claude/scripts/new-placeholder-tune.pl            renders every tune into content/audio
#   perl .claude/scripts/new-placeholder-tune.pl measure <wav>...   prints a file's peak, loudness and biggest jump
#
# Every note fades out before it ends, so a looped tune has no click where its end meets its start. A voice whose bars
# don't add up to the tune's length stops the script, since the loop would drift.
use strict; use warnings;

my $rate = 22050;
my %semitone = (C => 0, 'C#' => 1, D => 2, 'D#' => 3, E => 4, F => 5, 'F#' => 6, G => 7, 'G#' => 8, A => 9, 'A#' => 10, B => 11);

# Harshness lives in the high harmonics: a gentle low-pass keeps the old-console colour without the edge.
my $lowPassHz = 3200;

sub repeat_bar { my ($bar, $times) = @_; return join(' ', ($bar) x $times); }

# One bar of eighths from four notes, played twice.
sub arp { my ($notes) = @_; return join(' ', map { "$_:1" } (split(' ', $notes)) x 2); }

my @tunes = (
  { file => 'music/PlaceholderTown.wav', bpm => 96, peak => 0.26, voices => [
    { wave => 'pulse50', gain => 0.50, notes => 'E4:2 G4:2 C5:3 G4:1  D5:2 B4:2 G4:4  A4:2 C5:2 E5:3 C5:1  F4:2 A4:2 C5:4  G4:2 E4:2 G4:2 C5:2  A4:2 C5:2 A4:2 F4:2  G4:1 A4:1 B4:2 D5:2 B4:2  C5:6 R:2' },
    { wave => 'pulse25', gain => 0.14, notes => join(' ', map { arp($_) } 'C3 E3 G3 E3', 'G2 B2 D3 B2', 'A2 C3 E3 C3', 'F2 A2 C3 A2', 'C3 E3 G3 E3', 'F2 A2 C3 A2', 'G2 B2 D3 B2', 'C3 E3 G3 E3') },
    { wave => 'triangle', gain => 0.34, notes => 'C3:4 G2:4  G2:4 D3:4  A2:4 E3:4  F2:4 C3:4  C3:4 G2:4  F2:4 C3:4  G2:4 D3:4  C3:8' },
  ] },
  { file => 'music/PlaceholderForest.wav', bpm => 84, peak => 0.24, voices => [
    { wave => 'triangle', gain => 0.60, notes => 'A4:3 C5:1 B4:2 A4:2  G4:3 E4:1 G4:2 B4:2  A4:3 F4:1 A4:2 C5:2  B4:4 G4:2 E4:2  E5:3 C5:1 A4:4  F4:2 A4:2 D5:4  E5:2 D5:1 C5:1 B4:2 G#4:2  A4:6 R:2' },
    { wave => 'pulse25', gain => 0.12, notes => join(' ', map { arp($_) } 'A2 C3 E3 C3', 'E2 G2 B2 G2', 'F2 A2 C3 A2', 'E2 G2 B2 G2', 'A2 C3 E3 C3', 'D2 F2 A2 F2', 'E2 G#2 B2 G#2', 'A2 C3 E3 C3') },
    { wave => 'triangle', gain => 0.30, notes => 'A2:8 E2:8 F2:8 E2:8 A2:8 D2:8 E2:8 A2:8' },
  ] },
  { file => 'music/PlaceholderTitle.wav', bpm => 88, peak => 0.26, voices => [
    { wave => 'pulse50', gain => 0.50, notes => 'A4:2 C5:2 F5:4  D5:2 E5:1 F5:1 G5:4  E5:2 D5:2 B4:4  C5:2 E5:2 A4:4  A4:2 C5:2 F5:3 E5:1  D5:3 B4:1 G4:4  C5:2 E5:2 G5:2 E5:2  C5:6 R:2' },
    { wave => 'pulse25', gain => 0.13, notes => join(' ', map { arp($_) } 'F3 A3 C4 A3', 'G3 B3 D4 B3', 'E3 G3 B3 G3', 'A3 C4 E4 C4', 'F3 A3 C4 A3', 'G3 B3 D4 B3', 'C3 E3 G3 E3', 'C3 E3 G3 E3') },
    { wave => 'triangle', gain => 0.34, notes => 'F2:4 C3:4  G2:4 D3:4  E2:4 B2:4  A2:4 E3:4  F2:4 C3:4  G2:4 D3:4  C3:4 G2:4  C3:8' },
  ] },
  { file => 'music/PlaceholderBattle.wav', bpm => 150, peak => 0.28, voices => [
    { wave => 'pulse25', gain => 0.46, notes => 'A4:1 A4:1 C5:2 E5:2 C5:2  A4:1 A4:1 C5:2 F5:2 C5:2  B4:1 B4:1 D5:2 G5:2 D5:2  E5:3 D5:1 C5:2 A4:2  A4:1 A4:1 C5:2 E5:2 C5:2  A4:1 A4:1 C5:2 F5:2 C5:2  G4:1 B4:1 D5:2 G5:3 F5:1  E5:2 G#4:2 B4:2 E5:2' },
    { wave => 'triangle', gain => 0.42, notes => join(' ', map { repeat_bar($_, 1) } 'A2:1 A2:1 A3:1 A2:1 A2:1 A2:1 A3:1 A2:1', 'F2:1 F2:1 F3:1 F2:1 F2:1 F2:1 F3:1 F2:1', 'G2:1 G2:1 G3:1 G2:1 G2:1 G2:1 G3:1 G2:1', 'A2:1 A2:1 A3:1 A2:1 A2:1 A2:1 A3:1 A2:1', 'A2:1 A2:1 A3:1 A2:1 A2:1 A2:1 A3:1 A2:1', 'F2:1 F2:1 F3:1 F2:1 F2:1 F2:1 F3:1 F2:1', 'G2:1 G2:1 G3:1 G2:1 G2:1 G2:1 G3:1 G2:1', 'E2:1 E2:1 E3:1 E2:1 E2:1 E2:1 E3:1 E2:1') },
    { wave => 'noise', gain => 0.05, notes => repeat_bar('R:1 x:1', 32) },
  ] },
  { file => 'music/PlaceholderBoss.wav', bpm => 160, peak => 0.28, voices => [
    { wave => 'pulse25', gain => 0.46, notes => 'E4:2 G4:1 A4:1 B4:2 G4:2  C5:2 B4:1 A4:1 G4:2 E4:2  A4:2 C5:1 D5:1 E5:2 C5:2  D#5:3 B4:1 F#4:2 B4:2  E5:2 D5:1 C5:1 B4:2 G4:2  C5:2 E5:1 D5:1 C5:2 G4:2  A4:1 B4:1 C5:1 D5:1 E5:2 A4:2  B4:4 D#5:2 F#5:2' },
    { wave => 'triangle', gain => 0.44, notes => join(' ', map { repeat_bar($_, 1) } 'E2:1 E2:1 E3:1 E2:1 G2:1 E2:1 E3:1 E2:1', 'C2:1 C2:1 C3:1 C2:1 E2:1 C2:1 C3:1 C2:1', 'A2:1 A2:1 A3:1 A2:1 C3:1 A2:1 A3:1 A2:1', 'B2:1 B2:1 B3:1 B2:1 D#3:1 B2:1 B3:1 B2:1', 'E2:1 E2:1 E3:1 E2:1 G2:1 E2:1 E3:1 E2:1', 'C2:1 C2:1 C3:1 C2:1 E2:1 C2:1 C3:1 C2:1', 'A2:1 A2:1 A3:1 A2:1 C3:1 A2:1 A3:1 A2:1', 'B2:1 B2:1 B3:1 B2:1 D#3:1 B2:1 B3:1 B2:1') },
    { wave => 'noise', gain => 0.05, notes => repeat_bar('R:1 x:1', 32) },
  ] },
  # 12 eighths at 120 beats a minute: 3.0 s, BattleMusic.FanfareHold.
  { file => 'effects/PlaceholderVictory.wav', bpm => 120, peak => 0.26, voices => [
    { wave => 'pulse50', gain => 0.50, notes => 'G4:1 C5:1 E5:1 G5:3 E5:1 G5:5' },
    { wave => 'pulse25', gain => 0.16, notes => 'E4:1 G4:1 C5:1 E5:3 C5:1 E5:5' },
    { wave => 'triangle', gain => 0.36, notes => 'C3:3 G2:3 C3:6' },
  ] },
  { file => 'effects/PlaceholderDefeat.wav', bpm => 80, peak => 0.22, voices => [
    { wave => 'triangle', gain => 0.60, notes => 'A4:1 G4:1 F4:1 E4:1' },
    { wave => 'triangle', gain => 0.34, notes => 'A2:2 E2:2' },
  ] },
);

sub frequency_of {
  my ($note) = @_;
  $note =~ /^([A-G]#?)(\d)$/ or die "Not a note: $note\n";
  return 440 * 2 ** (($semitone{$1} + 12 * ($2 + 1) - 69) / 12);
}

sub wave_at {
  my ($wave, $phase) = @_;
  return $phase < 0.25 ? 1 : -1 if $wave eq 'pulse25';
  return $phase < 0.5 ? 1 : -1 if $wave eq 'pulse50';
  return 4 * abs($phase - 0.5) - 1;
}

# A soft attack, a slow fall as on old consoles, and silence by the note's end: no clicks between notes or at the loop.
sub envelope_at {
  my ($sample, $length) = @_;
  my $attack = 0.004 * $rate;
  my $release = 0.02 * $rate;
  my $shape = 1 - 0.35 * $sample / $length;
  $shape *= $sample / $attack if $sample < $attack;
  my $left = $length - 1 - $sample;
  $shape *= $left / $release if $left < $release;
  return $shape;
}

sub render_voice {
  my ($voice, $samplesPerEighth, $mix) = @_;
  my $at = 0;
  my $lfsr = 0x7FFF;
  for my $token (split ' ', $voice->{notes}) {
    my ($note, $eighths) = split /:/, $token;
    my $length = int($eighths * $samplesPerEighth);
    if ($note ne 'R') {
      # Noise is a short tick; everything else holds its note, most of its length.
      my $sounding = $voice->{wave} eq 'noise' ? int(0.03 * $rate) : int($length * 0.92);
      my $step = $voice->{wave} eq 'noise' ? 0 : frequency_of($note) / $rate;
      my $phase = 0;
      for my $sample (0 .. $sounding - 1) {
        my $value;
        if ($voice->{wave} eq 'noise') {
          my $bit = ($lfsr ^ ($lfsr >> 1)) & 1;
          $lfsr = ($lfsr >> 1) | ($bit << 14);
          $value = ($lfsr & 1) ? 1 : -1;
        } else {
          $value = wave_at($voice->{wave}, $phase);
          $phase += $step;
          $phase -= 1 if $phase >= 1;
        }
        $mix->[$at + $sample] += $value * $voice->{gain} * envelope_at($sample, $sounding);
      }
    }
    $at += $length;
  }
  return $at;
}

sub render_tune {
  my ($tune) = @_;
  my $samplesPerEighth = $rate * 30 / $tune->{bpm};
  my @mix;
  my %lengths;
  for my $voice (@{ $tune->{voices} }) {
    my $eighths = 0;
    $eighths += (split /:/)[1] for split ' ', $voice->{notes};
    $lengths{$eighths} = 1;
    render_voice($voice, $samplesPerEighth, \@mix);
  }
  die "$tune->{file}: its voices have different lengths: " . join(', ', sort keys %lengths) . "\n" if keys %lengths > 1;
  my ($eighths) = keys %lengths;
  my $count = int($eighths * $samplesPerEighth);
  my $alpha = 1 - exp(-2 * 3.14159265358979 * $lowPassHz / $rate);
  my ($state, $peak) = (0, 0);
  my @filtered;
  for my $index (0 .. $count - 1) {
    $state += $alpha * (($mix[$index] // 0) - $state);
    push @filtered, $state;
    $peak = abs($state) if abs($state) > $peak;
  }
  my $scale = $tune->{peak} / $peak;
  return pack('s<*', map { int($_ * $scale * 32767) } @filtered);
}

sub write_wav {
  my ($path, $data) = @_;
  my $length = length $data;
  open my $out, '>:raw', $path or die "$path: $!\n";
  print $out 'RIFF', pack('V', 36 + $length), 'WAVE', 'fmt ', pack('VvvVVvv', 16, 1, 1, $rate, $rate * 2, 2, 16), 'data', pack('V', $length), $data;
  close $out;
}

sub measure {
  my ($path) = @_;
  open my $in, '<:raw', $path or die "$path: $!\n";
  local $/;
  my $bytes = <$in>;
  close $in;
  my @samples = unpack('s<*', substr($bytes, 44));
  my ($peak, $sum, $jump) = (0, 0, 0);
  for my $index (0 .. $#samples) {
    $peak = abs($samples[$index]) if abs($samples[$index]) > $peak;
    $sum += $samples[$index] ** 2;
    my $step = abs($samples[$index] - $samples[$index - 1]);
    $jump = $step if $step > $jump;
  }
  printf "%-42s %5.1f s  peak %.3f  loudness %.3f  biggest jump %.3f  loop jump %.3f\n", $path, @samples / $rate, $peak / 32767,
    sqrt($sum / @samples) / 32767, $jump / 32767, abs($samples[0] - $samples[-1]) / 32767;
}

if (@ARGV && $ARGV[0] eq 'measure') {
  shift @ARGV;
  measure($_) for @ARGV;
  exit 0;
}

for my $tune (@tunes) {
  my $path = "content/audio/$tune->{file}";
  write_wav($path, render_tune($tune));
  measure($path);
}
