# Renders the placeholder jingles: short original tunes played by old-console voices (two pulses, a triangle, soft noise)
# into 16-bit PCM mono 22 050 Hz WAVs, the format the game plays. The game can't play MIDI, so the notes live here.
# Placeholders only, until the developer's real jingles; the music is the developer's own (decision 496).
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

my @tunes = (
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
  # The rate is the file's own: the freesound placeholders are 44 100 Hz, the tunes 22 050 Hz (ADR 0036).
  my $fileRate = unpack('V', substr($bytes, 24, 4));
  my @samples = unpack('s<*', substr($bytes, 44));
  my ($peak, $sum, $jump) = (0, 0, 0);
  for my $index (0 .. $#samples) {
    $peak = abs($samples[$index]) if abs($samples[$index]) > $peak;
    $sum += $samples[$index] ** 2;
    my $step = abs($samples[$index] - $samples[$index - 1]);
    $jump = $step if $step > $jump;
  }
  printf "%-42s %5.2f s  peak %.3f  loudness %.3f  biggest jump %.3f  loop jump %.3f\n", $path, @samples / $fileRate, $peak / 32767,
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
